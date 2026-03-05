using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.TimeLog;

// DTO: TimeLogResponseDto
// PURPOSE: Transfers time log response data between backend and frontend.
public class TimeLogResponseDto
{
    [JsonPropertyName("logId")]
    public Guid LogId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public TimeSpan? EndTime { get; set; }

    [JsonPropertyName("breakDuration")]
    public int BreakDuration { get; set; }

    [JsonPropertyName("totalHours")]
    public decimal TotalHours { get; set; }

    [JsonPropertyName("activity")]
    public string? Activity { get; set; }
}