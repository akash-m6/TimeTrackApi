using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Productivity;
using TimeTrack.API.Service;

namespace TimeTrack.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductivityController : ControllerBase
{
    private readonly IProductivityAnalyticsService _analyticsService;

    public ProductivityController(IProductivityAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Generates productivity report for the current user
    /// </summary>
    [HttpGet("my-report")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetMyProductivityReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var report = await _analyticsService.GenerateUserReportAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Generates productivity report for a specific user (managers/admins only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetUserProductivityReport(
        Guid userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _analyticsService.GenerateUserReportAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Generates productivity report for an entire department (managers/admins only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("department/{department}")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetDepartmentProductivityReport(
        string department,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _analyticsService.GenerateDepartmentReportAsync(department, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

    /// <summary>
    /// Calculates efficiency score for current user based on TimeTrack algorithm
    /// Formula: (Task-focused Time × 0.6) + (Completion Rate × 0.4)
    /// </summary>
    [HttpGet("my-efficiency")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetMyEfficiencyScore(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var score = await _analyticsService.CalculateEfficiencyScoreAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(score, "Efficiency score calculated"));
    }

    /// <summary>
    /// Calculates task completion rate percentage for current user
    /// </summary>
    [HttpGet("my-completion-rate")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetMyCompletionRate(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rate = await _analyticsService.CalculateTaskCompletionRateAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(rate, "Task completion rate calculated"));
    }
}