using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

// REPOSITORY: PendingRegistrationRepository
// PURPOSE: Handles database operations for PendingRegistration entities.
public class PendingRegistrationRepository : GenericRepository<PendingRegistration>, IPendingRegistrationRepository
{
    public PendingRegistrationRepository(TimeTrackDbContext context) : base(context)
    {
    }

    // METHOD: GetByEmailAsync
    // PURPOSE: Retrieves a pending registration by email.
    public async Task<PendingRegistration?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Email == email);
    }

    // METHOD: GetByStatusAsync
    // PURPOSE: Retrieves pending registrations by status.
    public async Task<IEnumerable<PendingRegistration>> GetByStatusAsync(string status)
    {
        return await _dbSet
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.AppliedDate)
            .ToListAsync();
    }

    // METHOD: GetPendingAsync
    // PURPOSE: Retrieves all registrations with status 'Pending'.
    public async Task<IEnumerable<PendingRegistration>> GetPendingAsync()
    {
        return await _dbSet
            .Where(r => r.Status == "Pending")
            .OrderBy(r => r.AppliedDate)
            .ToListAsync();
    }

    // METHOD: EmailExistsAsync
    // PURPOSE: Checks if a pending registration exists for the given email.
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(r => r.Email == email);
    }
}