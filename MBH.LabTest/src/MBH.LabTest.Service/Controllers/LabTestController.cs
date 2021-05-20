using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MBH.LabTest.Contracts;
using MBH.LabTest.Service.Dtos;
using MBH.LabTest.Service.Entities;
using MBH.Common;
using Microsoft.Extensions.Configuration;

namespace MBH.LabTest.Service.Controllers
{
    [ApiController]
    [Route("labTests")]
    public class LabTestController : ControllerBase
    {
        private readonly IRepository<LabTestItem> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IRepository<PatientItem> patientRepository;
        public LabTestController(IServiceProvider serviceProvider,  IRepository<LabTestItem> itemsRepository,IRepository<PatientItem> patientRepository, IPublishEndpoint publishEndpoint)
        {
            // var s=serviceProvider.GetServices<IRepository<LabTestItem>>();
            // var configuration = serviceProvider.GetService<IConfiguration>();
            // var PrefferedDb = configuration.GetSection("PrefferedDb").Get<string>();
            // var repo=s.Where(r=>r.Name==PrefferedDb).FirstOrDefault();
            // if(repo!=null)
            // this.itemsRepository=repo;
            // else
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
            this.patientRepository=patientRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LabTestItemDto>>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());

            return Ok(items);
        }

        // GET /items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LabTestItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        // POST /items
        [HttpPost]
        public async Task<ActionResult<LabTestItemDto>> PostAsync(CreateLabTestItemDto createItemDto)
        {
            var item = new LabTestItem
            {
                Name = createItemDto.Name,
                Value = createItemDto.Value,
                Price = createItemDto.Price,
                CreatedDate = DateTime.Now
            };

            await itemsRepository.CreateAsync(item);

           
            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        // PUT /items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateLabTestItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Value = updateItemDto.Value;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);

         

            return NoContent();
        }

        // DELETE /items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);

            return NoContent();
        }

        [HttpPost("AttachLabTest")]
        public async Task<ActionResult> PostAsync(AttachLabTestDto attachLabTest)
        {
            var patienttestItem = await patientRepository.GetAsync(
                item => item.PatientId == attachLabTest.PatientId && item.LabTestId == attachLabTest.LabTestId);

            if (patienttestItem == null)
            {
                var patient=await patientRepository.GetAsync(
                item => item.PatientId == attachLabTest.PatientId);
                string patientName=attachLabTest.PatientName;
                Guid patientId=Guid.NewGuid();
                if(patient!=null)
                {
                    patientId=attachLabTest.PatientId;
                    patientName=patient.Name;
                }
                patienttestItem = new PatientItem
                {
                    PatientId = patientId,
                    LabTestId = attachLabTest.LabTestId,
                    Name = patientName,
                    Note=attachLabTest.Note,
                    Value=attachLabTest.value,
                    TestDate = DateTime.UtcNow
                };

                await patientRepository.CreateAsync(patienttestItem);
                await publishEndpoint.Publish(new LabTestCreated(patienttestItem.PatientId,patienttestItem.LabTestId,patienttestItem.Value,patienttestItem.Name,patienttestItem.Note,patienttestItem.TestDate));
            }
            else
            {
               patienttestItem.Value=attachLabTest.value;
               patienttestItem.Note=attachLabTest.Note;
               patienttestItem.TestDate=DateTime.UtcNow;
               await patientRepository.UpdateAsync(patienttestItem);
               await publishEndpoint.Publish(new LabTestUpdated(patienttestItem.PatientId,patienttestItem.LabTestId,patienttestItem.Value,patienttestItem.Note,patienttestItem.TestDate));

            }

            return Ok();
        }
    }
}