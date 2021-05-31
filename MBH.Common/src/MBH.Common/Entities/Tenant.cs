using System;

namespace MBH.Common.Entities
{
    public class Tenant
    {
        public int TenantId { get; set; }
        public String DBType { get; set; }
        public String TenantName { get; set; }
        public string ConnectionString{get;set;}
        public string ServiceName{get;set;}
        public string Region {get;set;}
        public string AccessKey {get;set;}
        public string SecretKey {get;set;}
    }
}