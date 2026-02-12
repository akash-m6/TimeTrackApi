using TimeTrack.API.Models;

namespace TimeTrack.API.Repository.IRepository;

public interface ITimeLogRepository : IGenericRepository<TimeLogEntity>
{
    Task<IEnumerable<TimeLogEntity>> GetLogsByUserIdAsync(int userId);
    Task<IEnumerable<TimeLogEntity>> GetLogsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeLogEntity>> GetLogsByDepartmentAsync(string department, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalHoursByUserAsync(int userId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<TimeLogEntity>> GetPendingApprovalLogsAsync(int managerId);
    Task<TimeLogEntity> GetLogByUserAndDateAsync(int userId, DateTime date);
    Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<int> userIds, DateTime date);
}