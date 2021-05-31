using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MBH.Common.Settings;
using System;
using Amazon.Runtime;
using MBH.Common.Entities;
using System.Linq;

namespace MBH.Common.DynamoDB
{
    public static class Extensions
    {
        public static IServiceCollection AddDynamo(this IServiceCollection services, string tenantName)
        {

            services.AddSingleton(serviceProvider =>
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                var dynamoServiceSettings = configuration.GetSection(nameof(DynamoServiceSettings)).Get<DynamoServiceSettings>();
                var dynamoDbSettings = configuration.GetSection(nameof(DynamoDbSettings)).Get<DynamoDbSettings>();
                AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
                var tenants = configuration.GetSection("Tenants").Get<List<Tenant>>();
                var tenant = tenants.Where(t => t.TenantName == tenantName).FirstOrDefault();
                // Set the endpoint URL
                clientConfig.ServiceURL = tenant.ConnectionString;
                clientConfig.RegionEndpoint=Amazon.RegionEndpoint.GetBySystemName(tenant.Region);
                AWSCredentials aWSCredentials=new BasicAWSCredentials(tenant.AccessKey,tenant.SecretKey);
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(aWSCredentials, clientConfig);


                return client;
            });

            return services;
        }

        public static IServiceCollection AddDynamoRepository<T>(this IServiceCollection services, string collectionName, string tenantName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                AmazonDynamoDBClient client = serviceProvider.GetService<AmazonDynamoDBClient>();
                var configuration = serviceProvider.GetService<IConfiguration>();
                var dynamoServiceSettings = configuration.GetSection(nameof(DynamoServiceSettings)).Get<DynamoServiceSettings>();

                var request = new CreateTableRequest
                {
                    TableName = collectionName,
                    AttributeDefinitions = new List<AttributeDefinition>()
  {
    new AttributeDefinition
    {
      AttributeName = dynamoServiceSettings.AttirbuteName,
      AttributeType = dynamoServiceSettings.AttributeType
    }
  },
                    KeySchema = new List<KeySchemaElement>()
  {
    new KeySchemaElement
    {
      AttributeName = dynamoServiceSettings.AttirbuteName,
      KeyType = dynamoServiceSettings.KeyType  //Partition key
    }
  },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = dynamoServiceSettings.ReadCapacityUnits,
                        WriteCapacityUnits = dynamoServiceSettings.WriteCapacityUnits
                    }
                };
                bool tableExist = false;
                try
                {
                    var t = client.DescribeTableAsync(collectionName).Result;
                    tableExist = true;
                }
                catch
                {
                    tableExist = false;
                }
                if (!tableExist)
                {
                    try{
                    var response = client.CreateTableAsync(request).Result;
                    }
                    catch
                    {
                        
                    }
                }
                DynamoDBContext context = new DynamoDBContext(client);

                return new DynamoRepository<T>(client, context, tenantName);
            });

            return services;
        }
    }
}