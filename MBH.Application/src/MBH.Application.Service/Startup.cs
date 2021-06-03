using System;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MBH.Common.MassTransit;
using MBH.Common.MongoDB;
using MBH.Application.Service.Clients;
using Polly;
using Polly.Timeout;
using System.Collections.Generic;
using MBH.Common.Entities;
using MBH.Common.DynamoDB;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MBH.Common.Middleware;
using MassTransit;
using MBH.Common.Contracts;
using MBH.Application.Service.Consumers;
using RabbitMQ.Client;
using MBH.Common.Settings;
using MassTransit.Definition;
using MBH.Common;

namespace MBH.Application.Service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
            BsonSerializer.RegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
            var tenants = Configuration.GetSection("Tenants").Get<List<Tenant>>();
            foreach (var tenant in tenants)
            {
                if (tenant.ServiceName != "Application")
                {
                    if (tenant.DBType == "Mongo")
                    {
                        services.AddMongo(tenant.TenantName)
                              .AddMongoRepository<LabTestItem>("labTestItems", tenant.TenantName)
                                .AddMongoRepository<PatientItem>("patientLabTestItems", tenant.TenantName);

                    }
                    else
                    {
                        services.AddDynamo(tenant.TenantName)
                        .AddDynamoRepository<LabTestItem>("LabTestItem", tenant.TenantName)
                       .AddDynamoRepository<PatientItem>("PatientItem", tenant.TenantName);
                    }
                }
                else
                {
                    services.AddMongo(tenant.TenantName)
                        .AddMongoRepository<PatientInfo>("patientItems", tenant.TenantName);
                }
            }
            //services.AddMassTransitWithRabbitMq();
            services.AddMassTransit(configure =>
            {
                configure.UsingRabbitMq((context, configurator) =>
                {
                    foreach (var tenant in tenants)
                    {
                        if (tenant.TenantId > 0)
                        {
                            configurator.ReceiveEndpoint("tenant-" + tenant.TenantId + ".clinicianItem", x =>
                                {
                                    x.ConfigureConsumeTopology = false;

                                    x.Consumer(() => new ClinicianItemConsumer(context.GetService<IRepository<PatientInfo>>()));

                                    x.Bind("clinicianItem", s =>
                                    {
                                        s.RoutingKey = "tenant-" + tenant.TenantId + ".clinicianItem";
                                        s.ExchangeType = ExchangeType.Direct;
                                    });
                                });
                        }
                    }
                    var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMQSettings = Configuration.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                    // configurator.Host(rabbitMQSettings.Host);
                    configurator.Host(new System.Uri(string.Format("amqps://{0}:5671", rabbitMQSettings.Host)), h =>
                 {
                     h.Username("admin");
                     h.Password("Synerzip@2020");
                 });
                    configurator.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, false));
                });
            });

            services.AddMassTransitHostedService();
            services.AddHttpContextAccessor();
            AddCatalogClient(services);
            services.AddAuthentication(options =>
             {
                 options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                 options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
             }).AddJwtBearer(options =>
             {
                 options.Authority = "https://seecity.auth0.com/";
                 options.Audience = "https://seecity.adfs.com";
             });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MBH.Application.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MBH.Application.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseTenant();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void AddCatalogClient(IServiceCollection services)
        {
            Random jitterer = new Random();
            string baseAddress= Configuration.GetSection("BaseAddress").Get<string>();
            services.AddHttpClient<ClinicianClient>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timespan, retryAttempt) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<ClinicianClient>>()?
                        .LogWarning($"Delaying for {timespan.TotalSeconds} seconds, then making retry {retryAttempt}");
                }
            ))
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timespan) =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<ClinicianClient>>()?
                        .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
                },
                onReset: () =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    serviceProvider.GetService<ILogger<ClinicianClient>>()?
                        .LogWarning($"Closing the circuit...");
                }
            ))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }
    }
}
