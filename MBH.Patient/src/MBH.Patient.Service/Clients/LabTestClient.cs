using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MBH.Patient.Service.Dtos;

namespace MBH.Patient.Service.Clients
{
    public class LabTestClient
    {
        private readonly HttpClient httpClient;

        public LabTestClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<LabTestItemDto>> GetLabTestItemsAsync()
        {
            var items = await httpClient.GetFromJsonAsync<IReadOnlyCollection<LabTestItemDto>>("/labTests");
            return items;
        }
    }
}