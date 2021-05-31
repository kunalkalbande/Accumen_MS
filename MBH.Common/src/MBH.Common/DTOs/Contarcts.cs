using System;

namespace MBH.Common.Contracts
{
    public record ClinicianItem(Guid PatientId,Guid LabTestId,int Value, string Name, string Note,DateTime TestDate,int TenantId);


}