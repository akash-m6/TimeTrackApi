using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.TimeLog;

// DTO: CreateTimeLogDto
// PURPOSE: Transfers create time log data from frontend to backend.
public class CreateTimeLogDto
{
    [Required]
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [Required]
    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan? EndTime { get; set; }

    [Required]
    [JsonPropertyName("breakDuration")]
    public int BreakDuration { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [JsonPropertyName("totalHours")]
    public decimal TotalHours { get; set; }

    [JsonPropertyName("activity")]
    public string? Activity { get; set; }
}