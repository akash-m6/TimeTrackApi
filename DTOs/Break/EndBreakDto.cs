using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.Break;

// DTO: EndBreakDto
// PURPOSE: Transfers end break data from frontend to backend.
public class EndBreakDto
{
    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d):([0-5]\d)$", ErrorMessage = "EndTime must be in HH:mm:ss format")]
    [JsonPropertyName("endTime")]
    public string EndTime { get; set; } = string.Empty;
}
