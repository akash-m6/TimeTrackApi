namespace TimeTrack.API.DTOs.User;

public class DashboardStatsDto
{
    public int TotalTeamMembers { get; set; }
    public int ActiveTasks { get; set; }
    public decimal TotalTeamHoursToday { get; set; }
}
