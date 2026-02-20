using TimeTrack.API.DTOs.Auth;

namespace TimeTrack.API.Service;

public interface IAuthenticationService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(Guid userId, string email, string role);
}