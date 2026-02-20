using TimeTrack.API.Models;
using TaskEntity = TimeTrack.API.Models.TaskEntity;

namespace TimeTrack.API.Repository.IRepository;
    
public interface ITaskRepository : IGenericRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetTasksByAssignedUserAsync(Guid userId);
    Task<IEnumerable<TaskEntity>> GetTasksByCreatorAsync(Guid creatorId);
    Task<IEnumerable<TaskEntity>> GetTasksByStatusAsync(string status);
    Task<IEnumerable<TaskEntity>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskEntity>> GetTasksByDepartmentAsync(string department);
    Task<int> GetCompletedTasksCountAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<int> GetActiveTasksCountForUsersAsync(IEnumerable<Guid> userIds);
}