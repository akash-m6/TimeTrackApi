using TimeTrack.API.DTOs.Registration;
using TimeTrack.API.Models;

namespace TimeTrack.API.Service;

public interface IRegistrationService
{
    Task<PendingRegistration> ApplyForRegistrationAsync(RegistrationApplicationDto dto);
    Task<IEnumerable<PendingRegistration>> GetPendingRegistrationsAsync();
    Task<IEnumerable<PendingRegistration>> GetAllRegistrationsAsync(); // <-- Add this line
    Task<IEnumerable<PendingRegistration>> GetApprovedRegistrationsAsync();
    Task<IEnumerable<PendingRegistration>> GetRejectedRegistrationsAsync();
    Task<int> GetPendingCountAsync();
    Task<bool> ApproveRegistrationAsync(Guid registrationId, Guid approverId);
    Task<bool> RejectRegistrationAsync(Guid registrationId, Guid rejectorId, string reason);
    Task<bool> DeleteRegistrationAsync(Guid registrationId);
    Task<RegistrationResponseDto> SubmitRegistrationAsync(RegistrationRequestDto dto);
}