using TimeTrack.API.Models;

namespace TimeTrack.API.Repository.IRepository;

public interface IPendingRegistrationRepository : IGenericRepository<PendingRegistration>
{
    Task<PendingRegistration?> GetByEmailAsync(string email);
    Task<IEnumerable<PendingRegistration>> GetByStatusAsync(string status);
    Task<IEnumerable<PendingRegistration>> GetPendingAsync();
    Task<bool> EmailExistsAsync(string email);
}