using System;

namespace MBH.LabTest.Contracts
{
    public record LabTestCreated(Guid PatientId,Guid LabTestId,int Value, string Name, string Note,DateTime TestDate);
    public record LabTestUpdated(Guid PatientId,Guid LabTestId,int Value,string Note,DateTime TestDate);

    // public record LabTestUpdated(Guid LabTestId, string Name, string Value);

    // public record LabTestDeleted(Guid LabTestId);
}