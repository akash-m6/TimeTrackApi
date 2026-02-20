using TimeTrack.API.DTOs.Registration;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Service;

public class RegistrationService : IRegistrationService
{
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PendingRegistration> ApplyForRegistrationAsync(RegistrationApplicationDto dto)
    {
        // Check if email already exists
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        var existingApplication = await _unitOfWork.PendingRegistrations.GetByEmailAsync(dto.Email);
        if (existingApplication != null && existingApplication.Status == "Pending")
        {
            throw new InvalidOperationException("A pending registration already exists for this email");
        }

        var registration = new PendingRegistration
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Department = dto.Department,
            Status = "Pending",
            AppliedDate = DateTime.UtcNow
        };

        await _unitOfWork.PendingRegistrations.AddAsync(registration);
        await _unitOfWork.SaveChangesAsync();

        return registration;
    }

    public async Task<IEnumerable<PendingRegistration>> GetAllRegistrationsAsync()
    {
        return await _unitOfWork.PendingRegistrations.GetAllAsync();
    }

    public async Task<IEnumerable<PendingRegistration>> GetPendingRegistrationsAsync()
    {
        return await _unitOfWork.PendingRegistrations.GetPendingAsync();
    }

    public async Task<IEnumerable<PendingRegistration>> GetApprovedRegistrationsAsync()
    {
        return await _unitOfWork.PendingRegistrations.GetByStatusAsync("Approved");
    }

    public async Task<IEnumerable<PendingRegistration>> GetRejectedRegistrationsAsync()
    {
        return await _unitOfWork.PendingRegistrations.GetByStatusAsync("Rejected");
    }

    public async Task<int> GetPendingCountAsync()
    {
        var pending = await _unitOfWork.PendingRegistrations.GetPendingAsync();
        return pending.Count();
    }

    public async Task<bool> ApproveRegistrationAsync(Guid registrationId, Guid approverId)
    {
        var registration = await _unitOfWork.PendingRegistrations.GetByIdAsync(registrationId);
        if (registration == null || registration.Status != "Pending")
        {
            return false;
        }

        // Create actual user account
        var user = new User
        {
            Name = registration.Name,
            Email = registration.Email,
            PasswordHash = registration.PasswordHash,
            Role = registration.Role,
            Department = registration.Department,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);

        // Update registration status
        registration.Status = "Approved";
        registration.ProcessedDate = DateTime.UtcNow;
        registration.ProcessedByUserId = approverId;

        _unitOfWork.PendingRegistrations.Update(registration);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectRegistrationAsync(Guid registrationId, Guid rejectorId, string reason)
    {
        var registration = await _unitOfWork.PendingRegistrations.GetByIdAsync(registrationId);
        if (registration == null || registration.Status != "Pending")
        {
            return false;
        }

        registration.Status = "Rejected";
        registration.RejectionReason = reason;
        registration.ProcessedDate = DateTime.UtcNow;
        registration.ProcessedByUserId = rejectorId;

        _unitOfWork.PendingRegistrations.Update(registration);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteRegistrationAsync(Guid registrationId)
    {
        var registration = await _unitOfWork.PendingRegistrations.GetByIdAsync(registrationId);
        if (registration == null)
        {
            return false;
        }

        _unitOfWork.PendingRegistrations.Delete(registration);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<RegistrationResponseDto> SubmitRegistrationAsync(RegistrationRequestDto dto)
    {
        // 1) Basic validation
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return new RegistrationResponseDto
            {
                Success = false,
                Message = "Email and Password are required."
            };
        }

        // 2) Reject if a user already exists
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return new RegistrationResponseDto
            {
                Success = false,
                Message = "A user with this email already exists."
            };
        }

        // 3) Reject if there is a PENDING registration for the same email
        var existingApplication = await _unitOfWork.PendingRegistrations.GetByEmailAsync(dto.Email);
        if (existingApplication != null && string.Equals(existingApplication.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return new RegistrationResponseDto
            {
                Success = false,
                Message = "A pending registration already exists for this email.",
                RegistrationId = existingApplication.RegistrationId,
                Status = existingApplication.Status
            };
        }

        // 4) Create a fresh pending registration
        var pending = new PendingRegistration
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = string.IsNullOrWhiteSpace(dto.Role) ? "Employee" : dto.Role,
            Department = dto.Department,
            Status = "Pending",
            AppliedDate = DateTime.UtcNow
        };

        await _unitOfWork.PendingRegistrations.AddAsync(pending);
        await _unitOfWork.SaveChangesAsync();

        // 5) Build a friendly response
        return new RegistrationResponseDto
        {
            Success = true,
            Message = "Registration submitted successfully and is pending approval.",
            RegistrationId = pending.RegistrationId,
            Status = pending.Status,
            Email = pending.Email,
            Name = pending.Name,
            Role = pending.Role
        };
    }

}