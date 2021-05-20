using MBH.Common;
using MBH.LabTest.Service.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBH.LabTest.Service.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate next;
        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration,IRepository<LabTestItem> labtestItem )
        {
            var headers = context.Request.Headers;
            var tenantId = headers["tenantContext"];
            if(!string.IsNullOrEmpty(tenantId))
            {
            var s = context.RequestServices.GetServices<IRepository<LabTestItem>>();
            var tenants = configuration.GetSection("Tenants").Get<List<Tenant>>();
            var tenant = tenants.Where(t => t.TenantId.ToString() == tenantId).FirstOrDefault();
            if (tenant != null)
            {
                var repo = s.Where(r => r.Name == tenant.TenantName).FirstOrDefault();
                if (repo != null)
                {
                    context.Items["itemsRepository"] = repo;
                    labtestItem=repo;
                }
                else
                    context.Items["itemsRepository"] = s.LastOrDefault();
            }
            else
            {
               context.Items["itemsRepository"] = s.LastOrDefault();

            }
            }
            await next.Invoke(context);
        }
    }
    public static class TenantMiddlewareExtension
    {
        public static IApplicationBuilder UseTenant(this IApplicationBuilder app)
        {
            app.UseMiddleware<TenantMiddleware>();
            return app;
        }
    }
}