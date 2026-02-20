using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

public class PendingRegistrationRepository : GenericRepository<PendingRegistration>, IPendingRegistrationRepository
{
    public PendingRegistrationRepository(TimeTrackDbContext context) : base(context)
    {
    }

    public async Task<PendingRegistration?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Email == email);
    }

    public async Task<IEnumerable<PendingRegistration>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.AppliedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<PendingRegistration>> GetPendingAsync()
    {
        return await _dbSet
            .Where(r => r.Status == "Pending")
            .OrderBy(r => r.AppliedDate)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(r => r.Email == email);
    }
}