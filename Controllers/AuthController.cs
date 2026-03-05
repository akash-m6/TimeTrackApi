using Microsoft.AspNetCore.Mvc;
using TimeTrack.API.DTOs.Auth;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Registration;
using TimeTrack.API.Models.Enums;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: AuthController
// PURPOSE: Handles authentication and registration-related API requests from frontend.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IRegistrationService _registrationService;

    public AuthController(IAuthenticationService authService, IRegistrationService registrationService)
    {
        _authService = authService;
        _registrationService = registrationService;
    }



    // API ENDPOINT: POST /api/auth/login
    // CALLED FROM FRONTEND: login() function
    // PURPOSE: Authenticates user and returns login response.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Login successful"));
    }


    // API ENDPOINT: POST /api/auth/register
    // CALLED FROM FRONTEND: register() function
    // PURPOSE: Submits a registration request for Employee/Manager (requires admin approval).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // Employee and Manager registrations require admin approval
        if (request.Role.Equals("Employee", StringComparison.OrdinalIgnoreCase) ||
            request.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
        {
            // Route to pending registration (stored in PendingRegistrations table)
            var pendingRequest = new RegistrationRequestDto
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role,
                Department = request.Department
            };

            var pendingResult = await _registrationService.SubmitRegistrationAsync(pendingRequest);
            
            return Ok(ApiResponseDto<RegistrationResponseDto>.SuccessResponse(
                pendingResult, 
                "Registration submitted. Please wait for admin approval before logging in."));
        }

        // Block direct Admin registration through this endpoint
        return BadRequest(ApiResponseDto<string>.ErrorResponse(
            "Admin accounts cannot be created through self-registration."));
    }

    // API ENDPOINT: GET /api/auth/departments
    // CALLED FROM FRONTEND: getDepartments() function
    // PURPOSE: Retrieves available departments for registration dropdown.
    // FLOW: Controller → Service → Response to Frontend
    [HttpGet("departments")]
    public ActionResult<ApiResponseDto<IEnumerable<string>>> GetDepartments()
    {
        return Ok(ApiResponseDto<IEnumerable<string>>.SuccessResponse(
            DepartmentType.AllDepartments, 
            "Available departments retrieved"));
    }


    // API ENDPOINT: GET /api/auth/roles
    // CALLED FROM FRONTEND: getRoles() function
    // PURPOSE: Retrieves available roles for registration dropdown.
    // FLOW: Controller → Service → Response to Frontend
    [HttpGet("roles")]
    public ActionResult<ApiResponseDto<IEnumerable<string>>> GetRoles()
    {
        // Only Employee and Manager can self-register
        var roles = new[] { "Employee", "Manager" };
        return Ok(ApiResponseDto<IEnumerable<string>>.SuccessResponse(
            roles, 
            "Available roles retrieved"));
    }
}