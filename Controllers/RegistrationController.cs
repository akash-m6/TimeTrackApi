using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Registration;
using TimeTrack.API.Service;

namespace TimeTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    /// <summary>
    /// Submit a new registration request (Employee/Manager only)
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<PendingRegistrationDto>>> Register(
        [FromBody] RegistrationApplicationDto request)
    {
        var result = await _registrationService.ApplyForRegistrationAsync(request);
        return Ok(ApiResponseDto<PendingRegistrationDto>.SuccessResponse(MapToDto(result), "Registration submitted successfully"));
    }

    /// <summary>
    /// Get all registrations (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetAll()
    {
        var registrations = await _registrationService.GetAllRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "All registrations retrieved"));
    }

    /// <summary>
    /// Get pending registrations (Admin only)
    /// </summary>
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetPending()
    {
        var registrations = await _registrationService.GetPendingRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Pending registrations retrieved"));
    }

    /// <summary>
    /// Get approved registrations (Admin only)
    /// </summary>
    [HttpGet("approved")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetApproved()
    {
        var registrations = await _registrationService.GetApprovedRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Approved registrations retrieved"));
    }

    /// <summary>
    /// Get rejected registrations (Admin only)
    /// </summary>
    [HttpGet("rejected")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetRejected()
    {
        var registrations = await _registrationService.GetRejectedRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Rejected registrations retrieved"));
    }

    /// <summary>
    /// Get pending registration count (Admin only)
    /// </summary>
    [HttpGet("pending/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<int>>> GetPendingCount()
    {
        var count = await _registrationService.GetPendingCountAsync();
        return Ok(ApiResponseDto<int>.SuccessResponse(count, "Pending count retrieved"));
    }

    /// <summary>
    /// Approve a registration (Admin only)
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Approve(Guid id)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _registrationService.ApproveRegistrationAsync(id, adminUserId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Registration approved successfully"));
    }

    /// <summary>
    /// Reject a registration (Admin only)
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Reject(
        Guid id, [FromBody] RejectRegistrationDto request)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _registrationService.RejectRegistrationAsync(id, adminUserId, request.Reason);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Registration rejected successfully"));
    }

    /// <summary>
    /// Delete a registration record (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<string>>> Delete(Guid id)
    {
        await _registrationService.DeleteRegistrationAsync(id);
        return Ok(ApiResponseDto<string>.SuccessResponse("Deleted", "Registration deleted successfully"));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private static PendingRegistrationDto MapToDto(Models.PendingRegistration registration)
    {
        return new PendingRegistrationDto
        {
            RegistrationId = registration.RegistrationId,
            Name = registration.Name,
            Email = registration.Email,
            Role = registration.Role,
            Department = registration.Department,
            Status = registration.Status,
            AppliedDate = registration.AppliedDate,
            ProcessedDate = registration.ProcessedDate,
            RejectionReason = registration.RejectionReason
        };
    }
}