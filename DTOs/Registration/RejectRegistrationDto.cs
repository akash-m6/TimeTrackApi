using System.ComponentModel.DataAnnotations;

namespace TimeTrack.API.DTOs.Registration;

// DTO: RejectRegistrationDto
// PURPOSE: Transfers registration rejection data from frontend to backend.
// DTO: RejectRegistrationDto
// PURPOSE: Transfers registration rejection data from frontend to backend.
// DTO: RejectRegistrationDto
// PURPOSE: Transfers registration rejection data from frontend to backend.
public class RejectRegistrationDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
