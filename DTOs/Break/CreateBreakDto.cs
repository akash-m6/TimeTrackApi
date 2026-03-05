using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.Break;

// DTO: CreateBreakDto
// PURPOSE: Transfers create break data from frontend to backend.
public class CreateBreakDto
{
    [Required]
    [JsonPropertyName("timeLogId")]
    public Guid TimeLogId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    [JsonPropertyName("activity")]
    public string Activity { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "StartTime must be in HH:mm:ss format")]
    [JsonPropertyName("startTime")]
    public string StartTime { get; set; } = string.Empty;
}
