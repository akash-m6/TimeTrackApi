using TimeTrack.API.DTOs.Analytics;

namespace TimeTrack.API.Service.ServiceInterface;

public interface IAnalyticsService
{
    Task<TeamSummaryDto> GetTeamSummaryAsync(Guid managerId, DateTime? startDate, DateTime? endDate);
    Task<TeamHoursTrendDto> GetTeamHoursTrendAsync(Guid managerId, DateTime startDate, DateTime endDate, string groupBy);
    Task<TeamMemberPerformanceDto> GetTeamMemberPerformanceAsync(Guid managerId, DateTime? startDate, DateTime? endDate);
    Task<TaskCompletionBreakdownDto> GetTaskCompletionBreakdownAsync(Guid managerId, DateTime? startDate, DateTime? endDate);

    // Organization Analytics Methods
    Task<OrganizationAnalyticsResponse> GetOrganizationSummaryAsync(DateTime? startDate, DateTime? endDate, int? period);
    Task<DepartmentAnalyticsDto> GetDepartmentAnalyticsAsync(string departmentName, DateTime? startDate, DateTime? endDate);
    Task<List<DailyHoursDto>> GetHoursTrendAsync(int days);
}
