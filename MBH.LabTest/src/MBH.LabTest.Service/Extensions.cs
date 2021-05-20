using MBH.LabTest.Service.Dtos;
using MBH.LabTest.Service.Entities;

namespace MBH.LabTest.Service
{
    public static class Extensions
    {
        public static LabTestItemDto AsDto(this LabTestItem item)
        {
            return new LabTestItemDto(item.Id, item.Name, item.Value, item.Price, item.CreatedDate);
        }
    }
}