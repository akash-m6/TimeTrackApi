using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Data;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

public class UserRepository : GenericRepository<UserEntity>, IUserRepository
{
    public UserRepository(TimeTrackDbContext context) : base(context)
    {
    }

    public async Task<UserEntity> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<UserEntity>> GetUsersByRoleAsync(string role)
    {
        return await _dbSet.Where(u => u.Role == role && u.Status == "Active").ToListAsync();
    }

    public async Task<IEnumerable<UserEntity>> GetUsersByDepartmentAsync(string department)
    {
        return await _dbSet.Where(u => u.Department == department && u.Status == "Active").ToListAsync();
    }

    public async Task<IEnumerable<UserEntity>> GetActiveUsersAsync()
    {
        return await _dbSet.Where(u => u.Status == "Active").ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<int> GetEmployeesCountByManagerIdAsync(int managerId)
    {
        return await _dbSet.CountAsync(u => u.ManagerId == managerId);
    }
}

// Manager-Employee Self-Referencing Relationship
