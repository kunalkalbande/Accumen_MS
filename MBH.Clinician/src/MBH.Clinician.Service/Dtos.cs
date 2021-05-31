using System;
using System.ComponentModel.DataAnnotations;

namespace MBH.Clinician.Service.Dtos
{
    public record LabTestItemDto(Guid Id, string Name, string Value,decimal Price, DateTimeOffset CreatedDate);

    public record CreateLabTestItemDto([Required] string Name, string Value, [Range(0, 1000)] decimal Price);

    public record UpdateLabTestItemDto([Required] string Name, string Value, [Range(0, 1000)] decimal Price);
     public record AttachLabTestDto(Guid PatientId, Guid LabTestId, int value,string Note,string PatientName);
}