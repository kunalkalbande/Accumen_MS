using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace MBH.Common.DynamoDB
{

    public class DynamoRepository<T> : IRepository<T> where T : IEntity
    {
       private readonly AmazonDynamoDBClient _client;
       private readonly DynamoDBContext _context;
       public string Name {get;set;}
       public string Type {get;set;}
        public DynamoRepository(AmazonDynamoDBClient client, DynamoDBContext context,string tenantName)
        {
            Type="Dynamo";
            Name=tenantName;
            _client = client;
            _context=context;
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync()
        {
            return await _context.ScanAsync<T>(new List<ScanCondition>()).GetNextSetAsync();
        }

        public async Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter)
        {
         return (await _context.ScanAsync<T>(new List<ScanCondition>()).GetNextSetAsync()).FindAll(filter.Compile().Invoke);

        }

        public async Task<T> GetAsync(Guid id)
        {
            return await _context.LoadAsync<T>(id);
        }

        

        public async Task CreateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            entity.Id=Guid.NewGuid();
            await _context.SaveAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

           
            await _context.SaveAsync(entity);
        }

        public async Task RemoveAsync(Guid id)
        {
             await _context.DeleteAsync<T>(id);
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
        {
             return (await _context.ScanAsync<T>(new List<ScanCondition>()).GetNextSetAsync()).FindLast(filter.Compile().Invoke);
        }
    }
}