using System;

namespace MBH.Application.Service.Dtos
{
    public record AttachLabTestDto(Guid PatientId, Guid LabTestId, int value,string Note,string PatientName);

    public record PatientLabTestDto(Guid LabTestId, string Name, string refValue, int value,string note, DateTime TestDate);
    public record PatientInfoDto(Guid LabTestId, string Name, string refValue, int value,string note, DateTime TestDate,Guid patientId,string patietName);

    public record LabTestItemDto(Guid Id, string Name, string Value);
}