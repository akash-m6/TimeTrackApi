using TimeTrack.API.Models;

namespace TimeTrack.API.Repository.IRepository;

public interface ITaskTimeRepository : IGenericRepository<TaskTime>
{
    Task<IEnumerable<TaskTime>> GetTaskTimesByTaskIdAsync(Guid taskId);
    Task<IEnumerable<TaskTime>> GetTaskTimesByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<decimal> GetTotalHoursForTaskAsync(Guid taskId);
    Task<decimal> GetTotalHoursForUserAsync(Guid userId, DateTime startDate, DateTime endDate);
}