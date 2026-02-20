using TimeTrack.API.Models;

namespace TimeTrack.API.Repository.IRepository;

public interface IUserRepository : IGenericRepository<User>

{

    Task<User?> GetByEmailAsync(string email);

    Task<bool> EmailExistsAsync(string email);

    Task<IEnumerable<User>> GetActiveUsersAsync();

    Task<IEnumerable<User>> GetUsersByDepartmentAsync(string department);

    Task<User?> GetByIdWithManagerAsync(Guid userId);

    Task<IEnumerable<User>> GetAllWithManagerAsync();

    Task<IEnumerable<User>> GetEmployeesByManagerIdAsync(Guid managerId);

    Task<int> GetEmployeesCountByManagerIdAsync(Guid managerId);

}
