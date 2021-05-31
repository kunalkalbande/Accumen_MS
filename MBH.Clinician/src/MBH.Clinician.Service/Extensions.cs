using MBH.Clinician.Service.Dtos;
using MBH.Common.Entities;


namespace MBH.Clinician.Service
{
    public static class Extensions
    {
        public static LabTestItemDto AsDto(this LabTestItem item)
        {
            return new LabTestItemDto(item.Id, item.Name, item.Value, item.Price, item.CreatedDate);
        }
    }
}