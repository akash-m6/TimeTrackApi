using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.TimeLog;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: TimeLogController
// PURPOSE: Handles all time log-related API requests from frontend.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeLogController : ControllerBase
{
    private readonly ITimeLoggingService _timeLoggingService;
    private readonly IBreakService _breakService;
    private readonly ILogger<TimeLogController> _logger;

    public TimeLogController(ITimeLoggingService timeLoggingService, IBreakService breakService, ILogger<TimeLogController> logger)
    {
        _timeLoggingService = timeLoggingService;
        _breakService = breakService;
        _logger = logger;
    }

    // API ENDPOINT: POST /api/timelog
    // CALLED FROM FRONTEND: createTimeLog() function
    // PURPOSE: Creates a new time log for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> CreateTimeLog([FromBody] CreateTimeLogDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "[CreateTimeLog] UserId: {UserId}, Date: {Date}, StartTime: {StartTime}, EndTime: {EndTime}, BreakDuration: {BreakDuration}, TotalHours: {TotalHours}",
            userId, dto.Date, dto.StartTime, dto.EndTime, dto.BreakDuration, dto.TotalHours);

        try
        {
            var result = await _timeLoggingService.CreateTimeLogAsync(userId, dto);

            _logger.LogInformation(
                "[CreateTimeLog] Success - LogId: {LogId} generated for UserId: {UserId}",
                result.LogId, userId);

            return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result, "Time log created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, 
                "[CreateTimeLog] Failed - Duplicate log for UserId: {UserId}, Date: {Date}",
                userId, dto.Date);
            throw; // Re-throw to let GlobalExceptionHandler handle the response
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[CreateTimeLog] Database error for UserId: {UserId}. Exception: {ExceptionType}",
                userId, ex.GetType().Name);
            throw; // Re-throw to let GlobalExceptionHandler handle the response
        }
    }

    // API ENDPOINT: PUT /api/timelog/{logId}
    // CALLED FROM FRONTEND: updateTimeLog() function
    // PURPOSE: Updates an existing time log for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPut("{logId}")]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> UpdateTimeLog(Guid logId, [FromBody] CreateTimeLogDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation(
            "[UpdateTimeLog] LogId: {LogId}, UserId: {UserId}, Date: {Date}, StartTime: {StartTime}, EndTime: {EndTime}, BreakDuration: {BreakDuration}, TotalHours: {TotalHours}",
            logId, userId, dto.Date, dto.StartTime, dto.EndTime, dto.BreakDuration, dto.TotalHours);

        try
        {
            var result = await _timeLoggingService.UpdateTimeLogAsync(logId, userId, dto);

            _logger.LogInformation(
                "[UpdateTimeLog] Success - LogId: {LogId} updated by UserId: {UserId}",
                logId, userId);

            return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result, "Time log updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, 
                "[UpdateTimeLog] Failed - LogId: {LogId} not found for UserId: {UserId}",
                logId, userId);
            throw; // Re-throw to let GlobalExceptionHandler handle the response
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, 
                "[UpdateTimeLog] Failed - UserId: {UserId} attempted to update LogId: {LogId} they don't own",
                userId, logId);
            throw; // Re-throw to let GlobalExceptionHandler handle the response
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "[UpdateTimeLog] Database error for LogId: {LogId}, UserId: {UserId}. Exception: {ExceptionType}",
                logId, userId, ex.GetType().Name);
            throw; // Re-throw to let GlobalExceptionHandler handle the response
        }
    }

    // API ENDPOINT: DELETE /api/timelog/{logId}
    // CALLED FROM FRONTEND: deleteTimeLog() function
    // PURPOSE: Deletes a time log for the user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpDelete("{logId}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteTimeLog(Guid logId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.DeleteTimeLogAsync(logId, userId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Time log deleted successfully"));
    }

    // API ENDPOINT: GET /api/timelog/{logId}
    // CALLED FROM FRONTEND: getTimeLogById() function
    // PURPOSE: Gets a time log by its ID.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("{logId}")]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> GetTimeLogById(Guid logId)
    {
        var result = await _timeLoggingService.GetTimeLogByIdAsync(logId);
        return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result));
    }

    // API ENDPOINT: GET /api/timelog/user
    // CALLED FROM FRONTEND: getUserTimeLogs() function
    // PURPOSE: Gets all time logs for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("user")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TimeLogResponseDto>>>> GetUserTimeLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.GetUserTimeLogsAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<IEnumerable<TimeLogResponseDto>>.SuccessResponse(result));
    }

    // API ENDPOINT: POST /api/timelog/{logId}/approve
    // CALLED FROM FRONTEND: approveTimeLog() function
    // PURPOSE: Approves a time log (manager/admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPost("{logId}/approve")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ApproveTimeLog(Guid logId)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.ApproveTimeLogAsync(logId, managerId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Time log approved successfully"));
    }

    // API ENDPOINT: GET /api/timelog/total-hours
    // CALLED FROM FRONTEND: getTotalHours() function
    // PURPOSE: Gets total hours logged by the user in a date range.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("total-hours")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetTotalHours(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.CalculateTotalHoursAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(result));
    }

    // API ENDPOINT: GET /api/timelog/team/{managerId}
    // CALLED FROM FRONTEND: getTeamTimeLogs() function
    // PURPOSE: Gets all time logs for a manager's team.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("team/{managerId}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>>> GetTeamTimeLogs(Guid managerId)
    {
        var logs = await _timeLoggingService.GetTeamTimeLogsByManagerIdAsync(managerId);

        if (logs == null || !logs.Any())
            return NotFound(ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>.ErrorResponse("No team members or time logs found for the given manager."));

        return Ok(ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>.SuccessResponse(logs));
    }

    // API ENDPOINT: GET /api/timelog/{timeLogId}/breaks
    // CALLED FROM FRONTEND: getBreaksForTimeLog() function
    // PURPOSE: Gets all breaks for a specific time log.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("{timeLogId}/breaks")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DTOs.Break.BreakResponseDto>>>> GetBreaksForTimeLog(Guid timeLogId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        _logger.LogInformation("[GetBreaksForTimeLog] UserId: {UserId}, TimeLogId: {TimeLogId}", userId, timeLogId);

        try
        {
            var result = await _breakService.GetBreaksForTimeLogAsync(timeLogId, userId);
            return Ok(ApiResponseDto<IEnumerable<DTOs.Break.BreakResponseDto>>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "[GetBreaksForTimeLog] TimeLogId: {TimeLogId} not found", timeLogId);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "[GetBreaksForTimeLog] Unauthorized access to TimeLogId: {TimeLogId}", timeLogId);
            throw;
        }
    }

    // ==================== ORGANIZATION ANALYTICS ENDPOINTS ====================

    /// <summary>
    /// Gets all time log entries with department information (Admin only)
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TimeLogResponseDto>>>> GetAllTimeLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? departmentFilter,
        [FromQuery] string? status)
    {
        var result = await _timeLoggingService.GetAllTimeLogsWithDetailsAsync(
            startDate, 
            endDate, 
            departmentFilter, 
            status);

        return Ok(ApiResponseDto<IEnumerable<TimeLogResponseDto>>.SuccessResponse(result));
    }
}
