using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.TimeLog;
using TimeTrack.API.Service;

namespace TimeTrack.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimeLogController : ControllerBase
{
    private readonly ITimeLoggingService _timeLoggingService;

    public TimeLogController(ITimeLoggingService timeLoggingService)
    {
        _timeLoggingService = timeLoggingService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> CreateTimeLog([FromBody] CreateTimeLogDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.CreateTimeLogAsync(userId, dto);
        return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result, "Time log created successfully"));
    }

    [HttpPut("{logId}")]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> UpdateTimeLog(Guid logId, [FromBody] CreateTimeLogDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.UpdateTimeLogAsync(logId, userId, dto);
        return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result, "Time log updated successfully"));
    }

    [HttpDelete("{logId}")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeleteTimeLog(Guid logId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.DeleteTimeLogAsync(logId, userId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Time log deleted successfully"));
    }

    [HttpGet("{logId}")]
    public async Task<ActionResult<ApiResponseDto<TimeLogResponseDto>>> GetTimeLogById(Guid logId)
    {
        var result = await _timeLoggingService.GetTimeLogByIdAsync(logId);
        return Ok(ApiResponseDto<TimeLogResponseDto>.SuccessResponse(result));
    }

    [HttpGet("user")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TimeLogResponseDto>>>> GetUserTimeLogs(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.GetUserTimeLogsAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<IEnumerable<TimeLogResponseDto>>.SuccessResponse(result));
    }

    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPost("{logId}/approve")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ApproveTimeLog(Guid logId)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.ApproveTimeLogAsync(logId, managerId);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Time log approved successfully"));
    }

    [HttpGet("total-hours")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetTotalHours(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _timeLoggingService.CalculateTotalHoursAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(result));
    }

    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("team/{managerId}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>>> GetTeamTimeLogs(Guid managerId)
    {
        var logs = await _timeLoggingService.GetTeamTimeLogsByManagerIdAsync(managerId);

        if (logs == null || !logs.Any())
            return NotFound(ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>.ErrorResponse("No team members or time logs found for the given manager."));

        return Ok(ApiResponseDto<IEnumerable<DTOs.TimeLog.TeamTimeLogDto>>.SuccessResponse(logs));
    }
}