using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.TimeLog;

// DTO: TeamTimeLogDto
// PURPOSE: Transfers team time log data between backend and frontend.
public class TeamTimeLogDto
{
    [JsonPropertyName("logId")]
    public Guid LogId { get; set; }

    [JsonPropertyName("employeeId")]
    public Guid EmployeeId { get; set; }

    [JsonPropertyName("employeeName")]
    public string EmployeeName { get; set; }

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

    [JsonPropertyName("status")]
    public string Status { get; set; }
}
