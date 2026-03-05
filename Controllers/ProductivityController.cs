using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using TimeTrack.API.DTOs.Productivity;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.Service;

namespace TimeTrack.API.Controllers;

// CONTROLLER: ProductivityController
// PURPOSE: Handles all productivity-related API requests from frontend.
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProductivityController : ControllerBase
{
    private readonly IProductivityService _productivityService;
    private readonly ILogger<ProductivityController> _logger;

    public ProductivityController(IProductivityService productivityService, ILogger<ProductivityController> logger)
    {
        _productivityService = productivityService;
        _logger = logger;
    }


    // API ENDPOINT: GET /api/productivity/my-report
    // CALLED FROM FRONTEND: getMyProductivityReport() function
    // PURPOSE: Generates productivity report for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("my-report")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetMyProductivityReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var report = await _productivityService.GenerateUserReportAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

 
    // API ENDPOINT: GET /api/productivity/user/{userId}
    // CALLED FROM FRONTEND: getUserProductivityReport() function
    // PURPOSE: Generates productivity report for a specific user (managers/admins only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetUserProductivityReport(
        Guid userId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _productivityService.GenerateUserReportAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

 
    // API ENDPOINT: GET /api/productivity/department/{department}
    // CALLED FROM FRONTEND: getDepartmentProductivityReport() function
    // PURPOSE: Generates productivity report for an entire department (managers/admins only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("department/{department}")]
    public async Task<ActionResult<ApiResponseDto<ProductivityReportDto>>> GetDepartmentProductivityReport(
        string department,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var report = await _productivityService.GenerateDepartmentReportAsync(department, startDate, endDate);
        return Ok(ApiResponseDto<ProductivityReportDto>.SuccessResponse(report));
    }

  
    // API ENDPOINT: GET /api/productivity/my-efficiency
    // CALLED FROM FRONTEND: getMyEfficiencyScore() function
    // PURPOSE: Calculates efficiency score for current user based on TimeTrack algorithm.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("my-efficiency")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetMyEfficiencyScore(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var score = await _productivityService.CalculateEfficiencyScoreAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(score, "Efficiency score calculated"));
    }

   
    // API ENDPOINT: GET /api/productivity/my-completion-rate
    // CALLED FROM FRONTEND: getMyCompletionRate() function
    // PURPOSE: Calculates task completion rate percentage for current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("my-completion-rate")]
    public async Task<ActionResult<ApiResponseDto<decimal>>> GetMyCompletionRate(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var rate = await _productivityService.CalculateTaskCompletionRateAsync(userId, startDate, endDate);
        return Ok(ApiResponseDto<decimal>.SuccessResponse(rate, "Task completion rate calculated"));
    }

    // API ENDPOINT: GET /api/productivity
    // CALLED FROM FRONTEND: getProductivity() function
    // PURPOSE: Gets productivity summary for the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<ProductivityResponseDto>>> GetProductivity()
    {
        _logger.LogInformation("GET /api/Productivity called");

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("Unauthorized access to /api/Productivity - missing or invalid user id claim");
            return Unauthorized(ApiResponseDto<string>.ErrorResponse("Unauthorized"));
        }

        try
        {
            var result = await _productivityService.GetProductivityAsync(userId);
            return Ok(ApiResponseDto<ProductivityResponseDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get productivity for user {UserId}", userId);
            return StatusCode(500, ApiResponseDto<string>.ErrorResponse("An error occurred while calculating productivity."));
        }
    }
}