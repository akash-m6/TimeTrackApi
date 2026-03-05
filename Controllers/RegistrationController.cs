using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Registration;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: RegistrationController
// PURPOSE: Handles all registration-related API requests from frontend.
[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;

    public RegistrationController(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    // API ENDPOINT: POST /api/registration
    // CALLED FROM FRONTEND: submitRegistration() function
    // PURPOSE: Submits a new registration request.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponseDto<PendingRegistrationDto>>> Register(
        [FromBody] RegistrationApplicationDto request)
    {
        var result = await _registrationService.ApplyForRegistrationAsync(request);
        return Ok(ApiResponseDto<PendingRegistrationDto>.SuccessResponse(MapToDto(result), "Registration submitted successfully"));
    }

  
    // API ENDPOINT: GET /api/registration
    // CALLED FROM FRONTEND: getAllRegistrations() function
    // PURPOSE: Retrieves all registrations.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetAll()
    {
        var registrations = await _registrationService.GetAllRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "All registrations retrieved"));
    }

    // API ENDPOINT: GET /api/registration/pending
    // CALLED FROM FRONTEND: getPendingRegistrations() function
    // PURPOSE: Retrieves all pending registrations.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetPending()
    {
        var registrations = await _registrationService.GetPendingRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Pending registrations retrieved"));
    }

    // API ENDPOINT: GET /api/registration/approved
    // CALLED FROM FRONTEND: getApprovedRegistrations() function
    // PURPOSE: Retrieves all approved registrations.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("approved")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetApproved()
    {
        var registrations = await _registrationService.GetApprovedRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Approved registrations retrieved"));
    }


    // API ENDPOINT: GET /api/registration/rejected
    // CALLED FROM FRONTEND: getRejectedRegistrations() function
    // PURPOSE: Retrieves all rejected registrations.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("rejected")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<PendingRegistrationDto>>>> GetRejected()
    {
        var registrations = await _registrationService.GetRejectedRegistrationsAsync();
        var dtos = registrations.Select(MapToDto).ToList();
        return Ok(ApiResponseDto<IEnumerable<PendingRegistrationDto>>.SuccessResponse(dtos, "Rejected registrations retrieved"));
    }


    // API ENDPOINT: GET /api/registration/pending/count
    // CALLED FROM FRONTEND: getPendingCount() function
    // PURPOSE: Retrieves count of pending registrations.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("pending/count")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<int>>> GetPendingCount()
    {
        var count = await _registrationService.GetPendingCountAsync();
        return Ok(ApiResponseDto<int>.SuccessResponse(count, "Pending count retrieved"));
    }

    // API ENDPOINT: POST /api/registration/{id}/approve
    // CALLED FROM FRONTEND: approveRegistration() function
    // PURPOSE: Approves a registration.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Approve(Guid id)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _registrationService.ApproveRegistrationAsync(id, adminUserId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Registration approved successfully"));
    }

    // API ENDPOINT: POST /api/registration/{id}/reject
    // CALLED FROM FRONTEND: rejectRegistration() function
    // PURPOSE: Rejects a registration.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponseDto<bool>>> Reject(
        Guid id, [FromBody] RejectRegistrationDto request)
    {
        var adminUserId = GetCurrentUserId();
        var result = await _registrationService.RejectRegistrationAsync(id, adminUserId, request.Reason);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Registration rejected successfully"));
    }


    // API ENDPOINT: DELETE /api/registration/{id}
    // CALLED FROM FRONTEND: deleteRegistration() function
    // PURPOSE: Deletes a registration record.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
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