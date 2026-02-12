using TimeTrack.API.Models;

namespace TimeTrack.API.Repository.IRepository;

public interface ITaskRepository : IGenericRepository<TaskEntity>
{
    Task<IEnumerable<TaskEntity>> GetTasksByAssignedUserAsync(int userId);
    Task<IEnumerable<TaskEntity>> GetTasksByCreatorAsync(int creatorId);
    Task<IEnumerable<TaskEntity>> GetTasksByStatusAsync(string status);
    Task<IEnumerable<TaskEntity>> GetOverdueTasksAsync();
    Task<IEnumerable<TaskEntity>> GetTasksByDepartmentAsync(string department);
    Task<int> GetCompletedTasksCountAsync(int userId, DateTime startDate, DateTime endDate);
    Task<int> GetActiveTasksCountForUsersAsync(IEnumerable<int> userIds);
}