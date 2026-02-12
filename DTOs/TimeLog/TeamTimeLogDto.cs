namespace TimeTrack.API.DTOs.TimeLog;

public class TeamTimeLogDto
{
    public string EmployeeName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan BreakDuration { get; set; }
    public decimal TotalHours { get; set; }
}
