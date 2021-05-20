using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MBH.Common.Settings;

namespace MBH.Common.DynamoDB
{
    public static class Extensions
    {
        public static  IServiceCollection AddDynamo(this IServiceCollection services,IConfiguration configuration)
        {
         
            services.AddSingleton(serviceProvider =>
            {
                //var configuration = serviceProvider.GetService<IConfiguration>();
                var dynamoServiceSettings = configuration.GetSection(nameof(DynamoServiceSettings)).Get<DynamoServiceSettings>();
                var dynamoDbSettings = configuration.GetSection(nameof(DynamoDbSettings)).Get<DynamoDbSettings>();
                AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
                // Set the endpoint URL
                clientConfig.ServiceURL = dynamoDbSettings.ConnectionString;
                AmazonDynamoDBClient client = new AmazonDynamoDBClient(clientConfig);
                  var request = new CreateTableRequest
                {
                    TableName = dynamoServiceSettings.TableName,
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
                   var t= client.DescribeTableAsync(dynamoServiceSettings.TableName).Result;
                    tableExist = true;
                }
                catch
                {
                    tableExist = false;
                }
                if (!tableExist)
                {
                    var response =  client.CreateTableAsync(request).Result;
                }
                return client;  });

                return services;
        }

        public static IServiceCollection AddDynamoRepository<T>(this IServiceCollection services)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                AmazonDynamoDBClient client  = serviceProvider.GetService<AmazonDynamoDBClient>();
                 DynamoDBContext context = new DynamoDBContext(client);
                return new DynamoRepository<T>(client, context);
            });

            return services;
        }
    }
}