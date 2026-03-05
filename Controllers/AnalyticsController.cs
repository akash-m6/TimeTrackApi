using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Analytics;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: AnalyticsController
// PURPOSE: Handles all analytics-related API requests from frontend.
[Authorize(Policy = "ManagerOrAdmin")]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

   
    // API ENDPOINT: GET /api/analytics/team-summary
    // CALLED FROM FRONTEND: getTeamSummary() function
    // PURPOSE: Gets team summary analytics for dashboard cards.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("team-summary")]
    public async Task<ActionResult<ApiResponseDto<TeamSummaryDto>>> GetTeamSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _analyticsService.GetTeamSummaryAsync(managerId, startDate, endDate);
        return Ok(ApiResponseDto<TeamSummaryDto>.SuccessResponse(result, "Team summary retrieved successfully"));
    }

    
  
    // API ENDPOINT: GET /api/analytics/team-hours-trend
    // CALLED FROM FRONTEND: getTeamHoursTrend() function
    // PURPOSE: Gets team hours trend data for line chart.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("team-hours-trend")]
    public async Task<ActionResult<ApiResponseDto<TeamHoursTrendDto>>> GetTeamHoursTrend(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string groupBy = "day")
    {
        // Validate date range
        if (startDate > endDate)
        {
            return BadRequest(ApiResponseDto<TeamHoursTrendDto>.ErrorResponse("Start date cannot be after end date"));
        }

        // Validate groupBy parameter
        if (!new[] { "day", "week" }.Contains(groupBy.ToLower()))
        {
            return BadRequest(ApiResponseDto<TeamHoursTrendDto>.ErrorResponse("Invalid groupBy parameter. Use 'day' or 'week'"));
        }

        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _analyticsService.GetTeamHoursTrendAsync(managerId, startDate, endDate, groupBy);
        return Ok(ApiResponseDto<TeamHoursTrendDto>.SuccessResponse(result));
    }

 
    // API ENDPOINT: GET /api/analytics/team-member-performance
    // CALLED FROM FRONTEND: getTeamMemberPerformance() function
    // PURPOSE: Gets individual performance metrics for each team member.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("team-member-performance")]
    public async Task<ActionResult<ApiResponseDto<TeamMemberPerformanceDto>>> GetTeamMemberPerformance(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _analyticsService.GetTeamMemberPerformanceAsync(managerId, startDate, endDate);
        return Ok(ApiResponseDto<TeamMemberPerformanceDto>.SuccessResponse(result));
    }

   
    // API ENDPOINT: GET /api/analytics/task-completion-breakdown
    // CALLED FROM FRONTEND: getTaskCompletionBreakdown() function
    // PURPOSE: Gets task completion breakdown by status for doughnut chart.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("task-completion-breakdown")]
    public async Task<ActionResult<ApiResponseDto<TaskCompletionBreakdownDto>>> GetTaskCompletionBreakdown(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _analyticsService.GetTaskCompletionBreakdownAsync(managerId, startDate, endDate);
        return Ok(ApiResponseDto<TaskCompletionBreakdownDto>.SuccessResponse(result));
    }

    // ==================== ORGANIZATION ANALYTICS ENDPOINTS ====================

   
    // API ENDPOINT: GET /api/analytics/organization-summary
    // CALLED FROM FRONTEND: getOrganizationSummary() function
    // PURPOSE: Gets organization-wide analytics summary (Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("organization-summary")]
    public async Task<ActionResult<ApiResponseDto<OrganizationAnalyticsResponse>>> GetOrganizationSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? period)
    {
        // Validate period if provided
        if (period.HasValue && !new[] { 7, 14, 30, 90 }.Contains(period.Value))
        {
            return BadRequest(ApiResponseDto<OrganizationAnalyticsResponse>.ErrorResponse(
                "Invalid period. Use 7, 14, 30, or 90 days"));
        }

        var result = await _analyticsService.GetOrganizationSummaryAsync(startDate, endDate, period);
        return Ok(ApiResponseDto<OrganizationAnalyticsResponse>.SuccessResponse(
            result, 
            "Organization analytics retrieved successfully"));
    }

    
    // API ENDPOINT: GET /api/analytics/department/{departmentName}
    // CALLED FROM FRONTEND: getDepartmentAnalytics() function
    // PURPOSE: Gets detailed analytics for a specific department (Admin or Manager).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("department/{departmentName}")]
    public async Task<ActionResult<ApiResponseDto<DepartmentAnalyticsDto>>> GetDepartmentAnalytics(
        string departmentName,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            return BadRequest(ApiResponseDto<DepartmentAnalyticsDto>.ErrorResponse(
                "Department name is required"));
        }

        var result = await _analyticsService.GetDepartmentAnalyticsAsync(departmentName, startDate, endDate);
        return Ok(ApiResponseDto<DepartmentAnalyticsDto>.SuccessResponse(result));
    }

    
    // API ENDPOINT: GET /api/analytics/hours-trend
    // CALLED FROM FRONTEND: getHoursTrend() function
    // PURPOSE: Gets daily hours trend data for organization chart (Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("hours-trend")]
    public async Task<ActionResult<ApiResponseDto<List<DailyHoursDto>>>> GetHoursTrend(
        [FromQuery] int days = 7)
    {
        // Validate days parameter
        if (!new[] { 7, 14, 30, 90 }.Contains(days))
        {
            return BadRequest(ApiResponseDto<List<DailyHoursDto>>.ErrorResponse(
                "Invalid days parameter. Use 7, 14, 30, or 90"));
        }

        var result = await _analyticsService.GetHoursTrendAsync(days);
        return Ok(ApiResponseDto<List<DailyHoursDto>>.SuccessResponse(result));
    }
}
