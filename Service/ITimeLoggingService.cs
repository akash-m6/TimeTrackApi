using TimeTrack.API.DTOs.TimeLog;

namespace TimeTrack.API.Service;

public interface ITimeLoggingService
{
    Task<TimeLogResponseDto> CreateTimeLogAsync(int userId, CreateTimeLogDto dto);
    Task<TimeLogResponseDto> UpdateTimeLogAsync(int logId, int userId, CreateTimeLogDto dto);
    Task<bool> DeleteTimeLogAsync(int logId, int userId);
    Task<TimeLogResponseDto> GetTimeLogByIdAsync(int logId);
    Task<IEnumerable<TimeLogResponseDto>> GetUserTimeLogsAsync(int userId, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<TimeLogResponseDto>> GetDepartmentTimeLogsAsync(string department, DateTime startDate, DateTime endDate);
    Task<bool> ApproveTimeLogAsync(int logId, int managerId);
    Task<decimal> CalculateTotalHoursAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TeamTimeLogDto>> GetTeamTimeLogsByManagerIdAsync(int managerId);
    Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<int> userIds, DateTime date);
}