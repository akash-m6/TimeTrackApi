namespace TimeTrack.API.DTOs.Registration;

public class PendingRegistrationDto
{
    public Guid RegistrationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
    public string? RejectionReason { get; set; }
}