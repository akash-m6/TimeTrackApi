namespace TimeTrack.API.DTOs.TimeLog;

public class TimeLogResponseDto
{
    public Guid LogId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan BreakDuration { get; set; }
    public decimal TotalHours { get; set; }
    public string? Notes { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedDate { get; set; }
}