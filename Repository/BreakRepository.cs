using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: BreakRepository
// PURPOSE: Handles database operations for Break entities.
public class BreakRepository : GenericRepository<Break>, IBreakRepository
{
    public BreakRepository(TimeTrackDbContext context) : base(context)
    {
    }

    // METHOD: GetBreaksByTimeLogIdAsync
    // PURPOSE: Retrieves all breaks for a specific time log.
    public async Task<IEnumerable<Break>> GetBreaksByTimeLogIdAsync(Guid timeLogId)
    {
        return await _dbSet
            .Where(b => b.TimeLogId == timeLogId)
            .OrderBy(b => b.CreatedAt)
            .ToListAsync();
    }

    // METHOD: GetActiveBreakForTimeLogAsync
    // PURPOSE: Retrieves the active break for a specific time log.
    public async Task<Break?> GetActiveBreakForTimeLogAsync(Guid timeLogId)
    {
        return await _dbSet
            .Where(b => b.TimeLogId == timeLogId && b.EndTime == null)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }

    // METHOD: GetActiveBreakForUserAsync
    // PURPOSE: Retrieves the active break for a specific user.
    public async Task<Break?> GetActiveBreakForUserAsync(Guid userId)
    {
        return await _dbSet
            .Include(b => b.TimeLog)
            .Where(b => b.TimeLog.UserId == userId && b.EndTime == null)
            .OrderByDescending(b => b.CreatedAt)
            .FirstOrDefaultAsync();
    }
}
