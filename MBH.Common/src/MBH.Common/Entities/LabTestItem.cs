using System;

namespace MBH.Common.Entities
{
    public class LabTestItem : IEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string  Value { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}