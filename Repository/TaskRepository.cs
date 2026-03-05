using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: TaskRepository
// PURPOSE: Handles database operations for Task entities.
public class TaskRepository : GenericRepository<TaskEntity>, ITaskRepository
{
    public TaskRepository(TimeTrackDbContext context) : base(context)
    {
    }

    // METHOD: GetByIdAsync
    // PURPOSE: Retrieves a task by ID including related entities.
    public override async Task<TaskEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.TaskId == id);
    }

    // METHOD: GetTasksByAssignedUserAsync
    // PURPOSE: Retrieves tasks assigned to a specific user.
    public async Task<IEnumerable<TaskEntity>> GetTasksByAssignedUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Project)
            .Include(t => t.TaskTimes)
            .Where(t => t.AssignedToUserId == userId)
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    // METHOD: GetTasksByCreatorAsync
    // PURPOSE: Retrieves tasks created by a specific user.
    public async Task<IEnumerable<TaskEntity>> GetTasksByCreatorAsync(Guid creatorId)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Project)
            .Include(t => t.TaskTimes)
            .Where(t => t.CreatedByUserId == creatorId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    // METHOD: GetTasksByStatusAsync
    // PURPOSE: Retrieves tasks by status.
    public async Task<IEnumerable<TaskEntity>> GetTasksByStatusAsync(string status)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    // METHOD: GetOverdueTasksAsync
    // PURPOSE: Retrieves overdue tasks.
    public async Task<IEnumerable<TaskEntity>> GetOverdueTasksAsync()
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != "Completed")
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    // METHOD: GetTasksByDepartmentAsync
    // PURPOSE: Retrieves tasks by department.
    public async Task<IEnumerable<TaskEntity>> GetTasksByDepartmentAsync(string department)
    {
        return await _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Where(t => t.AssignedToUser.Department == department)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    // METHOD: GetCompletedTasksCountAsync
    // PURPOSE: Returns count of completed tasks for a user in a date range.
    public async Task<int> GetCompletedTasksCountAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.AssignedToUserId == userId 
                     && t.Status == "Completed" 
                     && t.CompletedDate.HasValue 
                     && t.CompletedDate >= startDate 
                     && t.CompletedDate <= endDate)
            .CountAsync();
    }

    // METHOD: GetActiveTasksCountForUsersAsync
    // PURPOSE: Returns count of active tasks for a list of users.
    public async Task<int> GetActiveTasksCountForUsersAsync(IEnumerable<Guid> userIds)
    {
        if (userIds == null || !userIds.Any()) return 0;
        return await _dbSet
            .Where(t => userIds.Contains(t.AssignedToUserId) && t.Status == "Active")
            .CountAsync();
    }

    // Organization Analytics Methods
    // METHOD: GetAllTasksWithDetailsAsync
    // PURPOSE: Retrieves all tasks with details for analytics.
    public async Task<IEnumerable<TaskEntity>> GetAllTasksWithDetailsAsync(
        DateTime? startDate, 
        DateTime? endDate, 
        string? status, 
        string? department)
    {
        var query = _dbSet
            .Include(t => t.AssignedToUser)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Project)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.CreatedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.CreatedDate <= endDate.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(department))
            query = query.Where(t => t.AssignedToUser.Department == department);

        return await query
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
    }

    // METHOD: GetTaskCountByStatusAsync
    // PURPOSE: Returns count of tasks by status in a date range.
    public async Task<int> GetTaskCountByStatusAsync(string status, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.Status == status && t.CreatedDate >= startDate && t.CreatedDate <= endDate)
            .CountAsync();
    }

    // METHOD: GetTaskCountsByDepartmentAsync
    // PURPOSE: Returns count of tasks grouped by department in a date range.
    public async Task<Dictionary<string, int>> GetTaskCountsByDepartmentAsync(DateTime startDate, DateTime endDate)
    {
        var result = await _dbSet
            .Include(t => t.AssignedToUser)
            .Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate 
                     && !string.IsNullOrEmpty(t.AssignedToUser.Department))
            .GroupBy(t => t.AssignedToUser.Department!)
            .Select(g => new { Department = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Department, x => x.Count);

        return result;
    }
}