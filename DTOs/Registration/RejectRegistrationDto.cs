using System.ComponentModel.DataAnnotations;

namespace TimeTrack.API.DTOs.Registration;

public class RejectRegistrationDto
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
