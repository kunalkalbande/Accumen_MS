using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MBH.Common;
using MBH.Patient.Service.Clients;
using MBH.Patient.Service.Dtos;
using MBH.Patient.Service.Entities;

namespace MBH.Patient.Service.Controllers
{
    [ApiController]
    [Route("patients")]
    public class PatiientsController : ControllerBase
    {
        private readonly IRepository<PatientItem> itemsRepository;
        private readonly LabTestClient  labTestClient;

        public PatiientsController(IRepository<PatientItem> itemsRepository, LabTestClient labTestClient)
        {
            this.itemsRepository = itemsRepository;
            this.labTestClient = labTestClient;
        }

        [HttpGet("GetPatientLabTest")]
        public async Task<ActionResult<IEnumerable<PatientLabTestDto>>> GetAsync(Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                return BadRequest();
            }

            var labTestItems = await labTestClient.GetLabTestItemsAsync();
            var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.PatientId == patientId);

            var patientLabTestDtos = inventoryItemEntities.Select(patientItem =>
            {
                var labTestItem = labTestItems.Single(labTestItem => labTestItem.Id == patientItem.LabTestId);
                return labTestItem.AsDto(patientItem.Value, patientItem.Note,patientItem.TestDate);
            });

            return Ok(patientLabTestDtos);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientItem>>> GetAsync()
        {
         
            var patientItemEntities = await itemsRepository.GetAllAsync();
            var patientItems=patientItemEntities.GroupBy(item => item.PatientId,(key,g) => g.OrderByDescending(x=>x.TestDate).First());

          
            return Ok(patientItems);
        }
        
    }
}