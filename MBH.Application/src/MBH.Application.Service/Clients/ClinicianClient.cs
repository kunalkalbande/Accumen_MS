using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using MBH.Application.Service.Dtos;

namespace MBH.Application.Service.Clients
{
    public class ClinicianClient
    {
        private readonly HttpClient httpClient;

        public ClinicianClient(HttpClient httpClient)
        {
            HttpClientHandler clientHandler = new HttpClientHandler();
            clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
    
            this.httpClient=new HttpClient(clientHandler);
            this.httpClient.BaseAddress=httpClient.BaseAddress;
        }

        public async Task<IReadOnlyCollection<LabTestItemDto>> GetLabTestItemsAsync(Microsoft.AspNetCore.Http.IHeaderDictionary headers)
        {
              foreach(var h in headers)
              {
                  try{
                   httpClient.DefaultRequestHeaders.Add(h.Key,h.Value[0]);
                  }
                  catch
                  {
                      
                  }
              }
             
            //httpClient.DefaultRequestHeaders.Add("authorization", "Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6InFiOXh6TzZYR1N4ZlllWm14b2lwUCJ9.eyJodHRwczovL2d1ZXN0eC5jb20vdXNlcl9tZXRhZGF0YSI6eyJmb3JjZVJlc2V0ICI6ImZhbHNlIiwidGVuYW50SWQiOjF9LCJodHRwczovL2d1ZXN0eC5jb20vcm9sZXMiOlsiQWRtaW4iXSwiaXNzIjoiaHR0cHM6Ly9zZWVjaXR5LmF1dGgwLmNvbS8iLCJzdWIiOiJhdXRoMHw1ZWJlZjA5YTc1Njc1YTBjNzQzMDU1ZDciLCJhdWQiOlsiaHR0cHM6Ly9zZWVjaXR5LmFkZnMuY29tIiwiaHR0cHM6Ly9zZWVjaXR5LmF1dGgwLmNvbS91c2VyaW5mbyJdLCJpYXQiOjE2MjE4NDAxNzEsImV4cCI6MTYyMTkyNjU3MSwiYXpwIjoiTzAyQW1ZcHEzSGJweXVIN0VveGlXMFlvRjZkMXJKQUwiLCJzY29wZSI6Im9wZW5pZCJ9.E7fvxs_l0O1-dnlRpATnFVc0TGbNnHOEFXsMiI797xRSSd4xxZCQYZ-Q63mdnVsT_B0ZyTiZ1rOMC9TwjPJr5lGvYIC0SdVF6Ez_epjosBCKcUyGUjlZ9_CkfibFYkekzlBt9WPKkbkMToVwPaQNe8e-Y-T96sV4uLYfGHkKT7aWGnKlWXE5j9wdomB0RltYHGrVeQF3zHAveKFUqdfh1h-Gm294HAoQjWWs66R3pSrjVdBcjucJaWRKl0CclNLVKRJsf9L7u05VogDzz_Djdoowt__FqPBEAp8jvOB-gpD6kzOTTHDWFP3MAQ7NctCvhXKLzTGBwjXNSxZIsY23lQ");
            var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<LabTestItemDto>>("/clinician/labTests");
            return items;
        }
        
    
    }
}