using System;

namespace MBH.Patient.Service.Dtos
{
    public record AttachLabTestDto(Guid PatientId, Guid LabTestId, int value,string Note,string PatientName);

    public record PatientLabTestDto(Guid LabTestId, string Name, string refValue, int value,string note, DateTime TestDate);

    public record LabTestItemDto(Guid Id, string Name, string Value);
}