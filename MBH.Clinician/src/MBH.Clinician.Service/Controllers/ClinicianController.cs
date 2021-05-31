using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MBH.Common.Contracts;
using MBH.Clinician.Service.Dtos;
using MBH.Common.Entities;
using MBH.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace MBH.Clinician.Service.Controllers
{
    [Authorize]
    [ApiController]
    [Route("clinician")]
    public class ClinicianController : ControllerBase
    {
        private readonly IRepository<LabTestItem> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;
        private readonly IRepository<PatientItem> patientRepository;
        private int tenantId;
        public ClinicianController(IHttpContextAccessor httpContextAccessor,IRepository<LabTestItem> itemsRepository,IRepository<PatientItem> patientRepository, IPublishEndpoint publishEndpoint)
        {
            try{
             var t=httpContextAccessor.HttpContext.Items["tenantId"];
            tenantId=t==null?0:Convert.ToInt32(t);
            }
            catch
            {

            }
            
            var item = httpContextAccessor.HttpContext.Items["itemsRepository"];
            var _patientsRepository = httpContextAccessor.HttpContext.Items["patientsRepository"];
            if (item != null)
            {
                this.itemsRepository = (IRepository<LabTestItem>)item;
            }
            else
            {
                this.itemsRepository = itemsRepository;
            }
            if (_patientsRepository != null)
            {
                this.patientRepository = (IRepository<PatientItem>)_patientsRepository;

            }
            else
            {
                this.patientRepository = patientRepository;

            }
            this.publishEndpoint = publishEndpoint;
        }

        [HttpGet("labTests")]
        public async Task<ActionResult<IEnumerable<LabTestItemDto>>> GetAsync()
        {
            var items = (await itemsRepository.GetAllAsync())
                        .Select(item => item.AsDto());

            return Ok(items);
        }

        // GET /items/{id}
        [HttpGet("labTests/{id}")]
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
        [HttpPost("labTests")]
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
        [HttpPut("labTests/{id}")]
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
        [HttpDelete("labTests/{id}")]
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
                var patient = await patientRepository.GetAsync(
                item => item.PatientId == attachLabTest.PatientId);
                string patientName = attachLabTest.PatientName;
                Guid patientId = Guid.NewGuid();
                if (patient != null)
                {
                    patientId = attachLabTest.PatientId;
                    patientName = patient.Name;
                }
                patienttestItem = new PatientItem
                {
                    PatientId = patientId,
                    LabTestId = attachLabTest.LabTestId,
                    Name = patientName,
                    Note = attachLabTest.Note,
                    Value = attachLabTest.value,
                    TestDate = DateTime.UtcNow,
                    TenantId = tenantId
                };

                await patientRepository.CreateAsync(patienttestItem);


            }
            else
            {
                patienttestItem.Value = attachLabTest.value;
                patienttestItem.Note = attachLabTest.Note;
                patienttestItem.TestDate = DateTime.UtcNow;
                patienttestItem.TenantId = tenantId;
                await patientRepository.UpdateAsync(patienttestItem);

            }
            await publishEndpoint.Publish(new ClinicianItem(patienttestItem.PatientId, patienttestItem.LabTestId, patienttestItem.Value, patienttestItem.Name, patienttestItem.Note, patienttestItem.TestDate, patienttestItem.TenantId));

            return Ok();
        }
    }
}