using MBH.Common;
using MBH.Common.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MBH.Common.Settings;

namespace MBH.Common.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate next;
        public TenantMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration,IServiceProvider serviceProvider)
        {
            var claimsIdentity=context.User.Identities;
            var someClaim = claimsIdentity.First().Claims.FirstOrDefault(x=>x.Type=="https://guestx.com/user_metadata");
            if(someClaim!=null)
            {
            var t=JsonConvert.DeserializeObject<Tenant>(someClaim.Value);
            var headers = context.Request.Headers;
            var tenantId = t.TenantId;
            if(tenantId>0)
            {
            var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
            var s = serviceProvider.GetServices<IRepository<LabTestItem>>();
            var patientServices = serviceProvider.GetServices<IRepository<PatientItem>>();
            var tenants = configuration.GetSection("Tenants").Get<List<Tenant>>();
            var tenant = tenants.Where(t => t.TenantId == tenantId).FirstOrDefault();
            if (tenant != null)
            {
                var repo = s.Where(r => r.Name == tenant.TenantName && r.Type==tenant.DBType).FirstOrDefault();
                var patientRepo=patientServices.Where(r => r.Name == tenant.TenantName&& r.Type==tenant.DBType).FirstOrDefault();
                if (repo != null)
                {
                    context.Items["tenantId"]=tenantId;
                    context.Items["itemsRepository"] = repo;
                    context.Items["patientsRepository"] = patientRepo;
                }
                else
                {
                    context.Items["tenantId"]=tenantId;
                    context.Items["itemsRepository"] = s.LastOrDefault();
                    context.Items["patientsRepository"] = patientServices.LastOrDefault();

                }
            }
            else
            {
               context.Items["tenantId"]=tenantId;
               context.Items["itemsRepository"] = s.LastOrDefault();
               context.Items["patientsRepository"] = patientServices.LastOrDefault();

            }
           
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