using Microsoft.EntityFrameworkCore;

using TimeTrack.API.Data;

using TimeTrack.API.Models;

using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Repository;

public class UserRepository : GenericRepository<User>, IUserRepository

{

    public UserRepository(TimeTrackDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)

    {

        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    }

    public async Task<bool> EmailExistsAsync(string email)

    {

        return await _dbSet.AnyAsync(u => u.Email.ToLower() == email.ToLower());

    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()

    {

        return await _dbSet

            .Include(u => u.Manager)

            .Include(u => u.AssignedEmployees)

            .Where(u => u.Status == "Active")

            .ToListAsync();

    }

    public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department)

    {

        return await _dbSet

            .Include(u => u.Manager)

            .Where(u => u.Department == department)

            .ToListAsync();

    }

    public async Task<User?> GetByIdWithManagerAsync(Guid userId)

    {

        return await _dbSet

            .Include(u => u.Manager)

            .Include(u => u.AssignedEmployees)

            .FirstOrDefaultAsync(u => u.UserId == userId);

    }

    public async Task<IEnumerable<User>> GetAllWithManagerAsync()

    {

        return await _dbSet

            .Include(u => u.Manager)

            .Include(u => u.AssignedEmployees)

            .Where(u => u.Status == "Active")

            .ToListAsync();

    }

    public async Task<IEnumerable<User>> GetEmployeesByManagerIdAsync(Guid managerId)

    {

        return await _dbSet

            .Where(u => u.ManagerId == managerId)

            .ToListAsync();

    }

    public async Task<int> GetEmployeesCountByManagerIdAsync(Guid managerId)

    {

        return await _dbSet.CountAsync(u => u.ManagerId == managerId);

    }

    // Organization Analytics Methods
    // METHOD: GetUsersByRoleAsync
    // PURPOSE: Retrieves users by role.
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
    {
        return await _dbSet
            .Where(u => u.Role == role && u.Status == "Active")
            .ToListAsync();
    }

    // METHOD: GetUserCountByRoleAsync
    // PURPOSE: Returns count of users by role.
    public async Task<int> GetUserCountByRoleAsync(string role)
    {
        return await _dbSet
            .CountAsync(u => u.Role == role && u.Status == "Active");
    }

    // METHOD: GetPunchedInUsersAsync
    // PURPOSE: Retrieves all currently active/punched-in users.
    public async Task<IEnumerable<User>> GetPunchedInUsersAsync()
    {
        return await _dbSet
            .Include(u => u.Manager)
            .Where(u => u.Status == "Active")
            .ToListAsync();
    }

    // METHOD: GetAllDepartmentsAsync
    // PURPOSE: Retrieves all distinct departments.
    public async Task<List<string>> GetAllDepartmentsAsync()
    {
        return await _dbSet
            .Where(u => !string.IsNullOrEmpty(u.Department))
            .Select(u => u.Department!)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();
    }

}


