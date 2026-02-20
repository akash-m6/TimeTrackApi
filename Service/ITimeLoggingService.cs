using TimeTrack.API.DTOs.TimeLog;

namespace TimeTrack.API.Service;

public interface ITimeLoggingService
{
    Task<TimeLogResponseDto> CreateTimeLogAsync(Guid userId, CreateTimeLogDto dto);
    Task<TimeLogResponseDto> UpdateTimeLogAsync(Guid logId, Guid userId, CreateTimeLogDto dto);
    Task<bool> DeleteTimeLogAsync(Guid logId, Guid userId);
    Task<TimeLogResponseDto> GetTimeLogByIdAsync(Guid logId);
    Task<IEnumerable<TimeLogResponseDto>> GetUserTimeLogsAsync(Guid userId, DateTime? startDate, DateTime? endDate);
    Task<IEnumerable<TimeLogResponseDto>> GetDepartmentTimeLogsAsync(string department, DateTime startDate, DateTime endDate);
    Task<bool> ApproveTimeLogAsync(Guid logId, Guid managerId);
    Task<decimal> CalculateTotalHoursAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TeamTimeLogDto>> GetTeamTimeLogsByManagerIdAsync(Guid managerId);
    Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<Guid> userIds, DateTime date);
}