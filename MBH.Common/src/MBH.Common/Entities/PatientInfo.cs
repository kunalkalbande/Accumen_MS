using System;
using MBH.Common;

namespace MBH.Common.Entities
{
    public class PatientInfo : IEntity
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }

        public String Name { get; set; }

        public Guid LabTestId { get; set; }

        public int Value { get; set; }
        public String Note { get; set; }

        public DateTime TestDate { get; set; }

        public int TenantId {get;set;}
    }
}