using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: TaskTimeRepository
// PURPOSE: Handles database operations for TaskTime entities.
public class TaskTimeRepository : GenericRepository<TaskTime>, ITaskTimeRepository
{
    public TaskTimeRepository(TimeTrackDbContext context) : base(context)
    {
    }

    // METHOD: GetTaskTimesByTaskIdAsync
    // PURPOSE: Retrieves all time logs for a specific task.
    public async Task<IEnumerable<TaskTime>> GetTaskTimesByTaskIdAsync(Guid taskId)
    {
        return await _dbSet
            .Include(tt => tt.User)
            .Include(tt => tt.Task)
            .Where(tt => tt.TaskId == taskId)
            .OrderBy(tt => tt.Date)
            .ToListAsync();
    }

    // METHOD: GetTaskTimesByUserIdAsync
    // PURPOSE: Retrieves all time logs for a user in a date range.
    public async Task<IEnumerable<TaskTime>> GetTaskTimesByUserIdAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(tt => tt.Task)
            .Where(tt => tt.UserId == userId && tt.Date >= startDate && tt.Date <= endDate)
            .OrderBy(tt => tt.Date)
            .ToListAsync();
    }

    // METHOD: GetTotalHoursForTaskAsync
    // PURPOSE: Returns total hours spent on a specific task.
    public async Task<decimal> GetTotalHoursForTaskAsync(Guid taskId)
    {
        return await _dbSet
            .Where(tt => tt.TaskId == taskId)
            .SumAsync(tt => tt.HoursSpent);
    }

    // METHOD: GetTotalHoursForUserAsync
    // PURPOSE: Returns total hours spent by a user in a date range.
    public async Task<decimal> GetTotalHoursForUserAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(tt => tt.UserId == userId && tt.Date >= startDate && tt.Date <= endDate)
            .SumAsync(tt => tt.HoursSpent);
    }
}