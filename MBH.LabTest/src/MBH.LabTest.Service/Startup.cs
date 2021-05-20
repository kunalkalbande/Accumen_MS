using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MBH.LabTest.Service.Entities;
using MBH.Common.MassTransit;
using MBH.Common.DynamoDB;
using MBH.Common.Settings;
using MBH.Common.MongoDB;
using Microsoft.AspNetCore.Http;

namespace MBH.LabTest.Service
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
           
             services.AddMongo()
                   .AddMongoRepository<LabTestItem>("labTestItems")
                     .AddMongoRepository<PatientItem>("patientLabTestItems")
                    .AddMassTransitWithRabbitMq();
                    services.AddHttpContextAccessor();
            // services.AddDynamo()
            // .AddDynamoRepository<LabTestItem>()
            //.AddMassTransitWithRabbitMq();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                   
           
            services.AddControllers(options =>
            {
                options.SuppressAsyncSuffixInActionNames = false;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MBH.LabTest.Service", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MBH.LabTest.Service v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();
            

            app.UseEndpoints(endpoints =>
            {
                
                endpoints.MapControllers();
            });
        }
    }
}
