using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

public class TimeLogRepository : GenericRepository<TimeLogEntity>, ITimeLogRepository
{
    public TimeLogRepository(TimeTrackDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TimeLogEntity>> GetLogsByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeLogEntity>> GetLogsByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.User)
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync();
    }

    public async Task<IEnumerable<TimeLogEntity>> GetLogsByDepartmentAsync(string department, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(t => t.User)
            .Where(t => t.User.Department == department && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalHoursByUserAsync(int userId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .SumAsync(t => t.TotalHours);
    }

    public async Task<IEnumerable<TimeLogEntity>> GetPendingApprovalLogsAsync(int managerId)
    {
        var managerDepartment = await _context.Users
            .Where(u => u.UserId == managerId)
            .Select(u => u.Department)
            .FirstOrDefaultAsync();

        return await _dbSet
            .Include(t => t.User)
            .Where(t => t.User.Department == managerDepartment && !t.IsApproved)
            .OrderBy(t => t.Date)
            .ToListAsync();
    }

    public async Task<TimeLogEntity> GetLogByUserAndDateAsync(int userId, DateTime date)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.UserId == userId && t.Date.Date == date.Date);
    }

    public async Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<int> userIds, DateTime date)
    {
        if (userIds == null || !userIds.Any()) return 0m;
        return await _dbSet
            .Where(t => userIds.Contains(t.UserId) && t.Date.Date == date.Date)
            .SumAsync(t => t.TotalHours);
    }
}