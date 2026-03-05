using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Break;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: BreakController
// PURPOSE: Handles all break-related API requests from frontend.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BreakController : ControllerBase
{
    private readonly IBreakService _breakService;
    private readonly ILogger<BreakController> _logger;

    public BreakController(IBreakService breakService, ILogger<BreakController> logger)
    {
        _breakService = breakService;
        _logger = logger;
    }

    // API ENDPOINT: POST /api/break
    // CALLED FROM FRONTEND: startBreak() function
    // PURPOSE: Starts a new break for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<BreakResponseDto>>> StartBreak([FromBody] CreateBreakDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "[StartBreak] UserId: {UserId}, TimeLogId: {TimeLogId}, Activity: {Activity}, StartTime: {StartTime}",
            userId, dto.TimeLogId, dto.Activity, dto.StartTime);

        try
        {
            var result = await _breakService.StartBreakAsync(userId, dto);

            _logger.LogInformation(
                "[StartBreak] Success - BreakId: {BreakId} created for TimeLogId: {TimeLogId}",
                result.BreakId, dto.TimeLogId);

            return StatusCode(201, ApiResponseDto<BreakResponseDto>.SuccessResponse(result, "Break started successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "[StartBreak] Failed - TimeLogId: {TimeLogId} not found",
                dto.TimeLogId);
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex,
                "[StartBreak] Failed - Active break already exists for TimeLogId: {TimeLogId}",
                dto.TimeLogId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[StartBreak] Error for UserId: {UserId}, TimeLogId: {TimeLogId}",
                userId, dto.TimeLogId);
            throw;
        }
    }

    // API ENDPOINT: PUT /api/break/{breakId}/end
    // CALLED FROM FRONTEND: endBreak() function
    // PURPOSE: Ends an active break for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPut("{breakId}/end")]
    public async Task<ActionResult<ApiResponseDto<BreakResponseDto>>> EndBreak(Guid breakId, [FromBody] EndBreakDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "[EndBreak] UserId: {UserId}, BreakId: {BreakId}, EndTime: {EndTime}",
            userId, breakId, dto.EndTime);

        try
        {
            var result = await _breakService.EndBreakAsync(breakId, userId, dto);

            _logger.LogInformation(
                "[EndBreak] Success - BreakId: {BreakId}, Duration: {Duration} minutes",
                breakId, result.Duration);

            return Ok(ApiResponseDto<BreakResponseDto>.SuccessResponse(result, "Break ended successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex,
                "[EndBreak] Failed - BreakId: {BreakId} not found",
                breakId);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex,
                "[EndBreak] Failed - Invalid end time for BreakId: {BreakId}",
                breakId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[EndBreak] Error for BreakId: {BreakId}, UserId: {UserId}",
                breakId, userId);
            throw;
        }
    }

    // API ENDPOINT: GET /api/break/active
    // CALLED FROM FRONTEND: getActiveBreak() function
    // PURPOSE: Gets the currently active break for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponseDto<BreakResponseDto?>>> GetActiveBreak()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation("[GetActiveBreak] UserId: {UserId}", userId);

        var result = await _breakService.GetActiveBreakForUserAsync(userId);

        if (result == null)
        {
            _logger.LogInformation("[GetActiveBreak] No active break found for UserId: {UserId}", userId);
            return Ok(ApiResponseDto<BreakResponseDto?>.SuccessResponse(null, "No active break found"));
        }

        _logger.LogInformation(
            "[GetActiveBreak] Found active break - BreakId: {BreakId} for UserId: {UserId}",
            result.BreakId, userId);

        return Ok(ApiResponseDto<BreakResponseDto?>.SuccessResponse(result));
    }

    // API ENDPOINT: DELETE /api/break/{breakId}
    // CALLED FROM FRONTEND: deleteBreak() function
    // PURPOSE: Deletes a break for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpDelete("{breakId}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteBreak(Guid breakId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation("[DeleteBreak] UserId: {UserId}, BreakId: {BreakId}", userId, breakId);

        try
        {
            var result = await _breakService.DeleteBreakAsync(breakId, userId);

            if (result)
            {
                _logger.LogInformation(
                    "[DeleteBreak] Success - BreakId: {BreakId} deleted by UserId: {UserId}",
                    breakId, userId);
                return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Break deleted successfully"));
            }

            _logger.LogWarning("[DeleteBreak] Failed - BreakId: {BreakId} not found", breakId);
            return NotFound(ApiResponseDto<bool>.ErrorResponse("Break not found"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex,
                "[DeleteBreak] Failed - UserId: {UserId} attempted to delete BreakId: {BreakId} they don't own",
                userId, breakId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[DeleteBreak] Error for BreakId: {BreakId}, UserId: {UserId}",
                breakId, userId);
            throw;
        }
    }
}
