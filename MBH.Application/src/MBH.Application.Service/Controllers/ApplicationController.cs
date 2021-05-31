using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MBH.Common;
using MBH.Application.Service.Clients;
using MBH.Application.Service.Dtos;
using MBH.Common.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MBH.Application.Service.Controllers
{
    [Authorize]
    [ApiController]
    [Route("application")]
    public class ApplicationController : ControllerBase
    {
        private readonly IRepository<PatientInfo> itemsRepository;
        private readonly IRepository<LabTestItem> labTestItem;
        private readonly ClinicianClient  clinicianClient;
        private int _tenantId=0;
        public ApplicationController(IHttpContextAccessor httpContextAccessor,IRepository<PatientInfo> itemsRepository, ClinicianClient clinicianClient,IRepository<LabTestItem> labTestItem)
        {
               var t=httpContextAccessor.HttpContext.Items["tenantId"];
            _tenantId=t==null?0:Convert.ToInt32(t);
            var item = httpContextAccessor.HttpContext.Items["itemsRepository"];

            if (item != null)
            {
                this.labTestItem = (IRepository<LabTestItem>)item;
            }
            else
            {
                this.labTestItem = labTestItem;
            }
            this.itemsRepository = itemsRepository;
            this.clinicianClient = clinicianClient;
        }

        [HttpGet("getPatientLabTest")]
        public async Task<ActionResult<IEnumerable<PatientLabTestDto>>> GetAsync(Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                return BadRequest();
            }

            var labTestItems = await clinicianClient.GetLabTestItemsAsync(HttpContext.Request.Headers);
            var inventoryItemEntities = await itemsRepository.GetAllAsync(item => item.PatientId == patientId);

            var patientLabTestDtos = inventoryItemEntities.Select(patientItem =>
            {
                var labTestItem = labTestItems.Single(labTestItem => labTestItem.Id == patientItem.LabTestId);
                return labTestItem.AsDto(patientItem.Value, patientItem.Note,patientItem.TestDate);
            });

            return Ok(patientLabTestDtos);
        }
         [HttpGet("getPatientInfo")]
        public async Task<ActionResult<IEnumerable<PatientLabTestDto>>> GetPatientInfoAsync()
        {
          

            var labTestItems =await labTestItem.GetAllAsync();
            var patientItemEntities = await itemsRepository.GetAllAsync(item => item.TenantId == _tenantId);
           // var inventoryItemEntities = patientItemEntities.GroupBy(item => item.PatientId,(key,g) => g.OrderByDescending(x=>x.TestDate).First());

            var patientLabTestDtos = patientItemEntities.Select(patientItem =>
            {
                var labTestItem = labTestItems.SingleOrDefault(labTestItem => labTestItem.Id == patientItem.LabTestId);
                if(labTestItem==null)
                labTestItem=new LabTestItem();
                return labTestItem.AsDto(patientItem.Value, patientItem.Note,patientItem.TestDate,patientItem.PatientId,patientItem.Name);
            });

            return Ok(patientLabTestDtos);
        }
        [HttpGet("getPatients")]
        public async Task<ActionResult<IEnumerable<PatientItem>>> GetAsync()
        {
         
            var patientItemEntities = await itemsRepository.GetAllAsync();
            var patientItems=patientItemEntities.GroupBy(item => item.PatientId,(key,g) => g.OrderByDescending(x=>x.TestDate).First());

          
            return Ok(patientItems);
        }

         [HttpGet("getPatientByTenantId")]
        public async Task<ActionResult<IEnumerable<PatientItem>>> GetAsync(int tenantId)
        {
            var patientItemEntities = await itemsRepository.GetAllAsync(item => item.TenantId == tenantId);

           var patientItems=patientItemEntities.GroupBy(item => item.PatientId,(key,g) => g.OrderByDescending(x=>x.TestDate).First());

            return Ok(patientItemEntities);
        }
        
    }
}