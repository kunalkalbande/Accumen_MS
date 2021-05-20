using System;
using MBH.Patient.Service.Dtos;
using MBH.Patient.Service.Entities;

namespace MBH.Patient.Service
{
    public static class Extensions
    {
        public static PatientLabTestDto AsDto(this LabTestItemDto item, int value, string note,DateTime testDate)
        {
            return new PatientLabTestDto(item.Id, item.Name, item.Value, value, note,testDate);
        }
    }
}