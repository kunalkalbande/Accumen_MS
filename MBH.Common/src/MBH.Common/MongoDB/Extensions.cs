using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MBH.Common.Settings;
using System.Collections.Generic;
using MBH.Common.Entities;
using System.Linq;

namespace MBH.Common.MongoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services,string tenantName)
        {
            
            
            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var mongoDbSettings = configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                var tenants = configuration.GetSection("Tenants").Get<List<Tenant>>();
                var tenant = tenants.Where(t => t.TenantName == tenantName).FirstOrDefault();
                var mongoClient = new MongoClient(tenant.ConnectionString);
                return mongoClient.GetDatabase(tenant.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName,string tenantName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var databases = serviceProvider.GetServices<IMongoDatabase>();
                var tenants = configuration.GetSection("Tenants").Get<List<Tenant>>();
                var tenant = tenants.Where(t => t.TenantName == tenantName).FirstOrDefault();
                var database=databases.First(d=>d.DatabaseNamespace.DatabaseName==tenant.ServiceName);
                return new MongoRepository<T>(database, collectionName,tenantName);
            });

            return services;
        }
    }
}