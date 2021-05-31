using System;
using MBH.Application.Service.Dtos;
using MBH.Common.Entities;

namespace MBH.Application.Service
{
    public static class Extensions
    {
        public static PatientLabTestDto AsDto(this LabTestItemDto item, int value, string note,DateTime testDate)
        {
            return new PatientLabTestDto(item.Id, item.Name, item.Value, value, note,testDate);
        }
         public static PatientInfoDto AsDto(this LabTestItem item, int value, string note,DateTime testDate, Guid patientId, string name)
        {
            return new PatientInfoDto(item.Id, item.Name, item.Value, value, note,testDate,patientId,name);
        }
    }
}