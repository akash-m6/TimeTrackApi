using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.User;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Controllers;

// CONTROLLER: UserController
// PURPOSE: Handles all user-related API requests from frontend.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UserController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

  
    // API ENDPOINT: GET /api/user/profile
    // CALLED FROM FRONTEND: getProfile() function
    // PURPOSE: Gets current user profile information.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Users.GetByIdWithManagerAsync(userId);

        if (user == null)
            return NotFound(ApiResponseDto<UserDto>.ErrorResponse("User not found"));

        return Ok(ApiResponseDto<UserDto>.SuccessResponse(MapToDto(user)));
    }


    // API ENDPOINT: GET /api/user/all
    // CALLED FROM FRONTEND: getAllUsers() function
    // PURPOSE: Gets all active users in the system.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserDto>>>> GetAllUsers()
    {
        var users = await _unitOfWork.Users.GetAllWithManagerAsync();
        return Ok(ApiResponseDto<IEnumerable<UserDto>>.SuccessResponse(users.Select(MapToDto)));
    }

 
    // API ENDPOINT: GET /api/user/{userId}
    // CALLED FROM FRONTEND: getUserById() function
    // PURPOSE: Gets a user by their ID.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> GetUserById(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdWithManagerAsync(userId);
        if (user == null)
            return NotFound(ApiResponseDto<UserDto>.ErrorResponse("User not found"));

        return Ok(ApiResponseDto<UserDto>.SuccessResponse(MapToDto(user)));
    }


    // API ENDPOINT: PUT /api/user/{userId}
    // CALLED FROM FRONTEND: updateUser() function
    // PURPOSE: Updates a user.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpPut("{userId}")]
    public async Task<ActionResult<ApiResponseDto<UserDto>>> UpdateUser(Guid userId, [FromBody] UserUpdateDto dto)
    {
        var user = await _unitOfWork.Users.GetByIdWithManagerAsync(userId);
        if (user == null)
            return NotFound(ApiResponseDto<UserDto>.ErrorResponse("User not found"));

        var oldRole = user.Role;
        var newRole = dto.Role ?? oldRole;

        if (!string.IsNullOrEmpty(dto.FullName)) user.Name = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Department)) user.Department = dto.Department;
        if (!string.IsNullOrEmpty(dto.Status)) user.Status = dto.Status;
        if (!string.IsNullOrEmpty(dto.Role)) user.Role = dto.Role;

        // Manager → Employee
        if (oldRole == "Manager" && newRole == "Employee")
        {
            var employees = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(userId);
            foreach (var emp in employees)
            {
                emp.ManagerId = null;
                _unitOfWork.Users.Update(emp);
            }
            user.ManagerId = null;
        }

        // Employee → Manager
        if (oldRole == "Employee" && newRole == "Manager")
            user.ManagerId = null;

        // Employee: assign ONE manager
        if (newRole == "Employee" && dto.ManagerId.HasValue)
            user.ManagerId = dto.ManagerId.Value == Guid.Empty ? null : dto.ManagerId.Value;

        // Manager: assign MANY employees
        if (newRole == "Manager" && dto.AssignedEmployeeIds != null)
        {
            var currentEmployees = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(userId);
            foreach (var emp in currentEmployees)
            {
                if (!dto.AssignedEmployeeIds.Contains(emp.UserId))
                {
                    emp.ManagerId = null;
                    _unitOfWork.Users.Update(emp);
                }
            }

            foreach (var empId in dto.AssignedEmployeeIds)
            {
                var emp = await _unitOfWork.Users.GetByIdAsync(empId);
                if (emp != null && emp.Role == "Employee" && emp.UserId != userId)
                {
                    emp.ManagerId = userId;
                    _unitOfWork.Users.Update(emp);
                }
            }
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        user = await _unitOfWork.Users.GetByIdWithManagerAsync(userId);
        return Ok(ApiResponseDto<UserDto>.SuccessResponse(MapToDto(user!), "User updated"));
    }

    // API ENDPOINT: GET /api/user/department/{department}
    // CALLED FROM FRONTEND: getUsersByDepartment() function
    // PURPOSE: Gets all users in a specific department.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("department/{department}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserDto>>>> GetUsersByDepartment(string department)
    {
        var users = await _unitOfWork.Users.GetUsersByDepartmentAsync(department);
        return Ok(ApiResponseDto<IEnumerable<UserDto>>.SuccessResponse(users.Select(MapToDto)));
    }


    // API ENDPOINT: GET /api/user/{managerId}/employees
    // CALLED FROM FRONTEND: getEmployeesByManager() function
    // PURPOSE: Gets all employees under a specific manager.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("{managerId}/employees")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserDto>>>> GetEmployeesByManager(Guid managerId)
    {
        var employees = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);
        return Ok(ApiResponseDto<IEnumerable<UserDto>>.SuccessResponse(employees.Select(MapToDto)));
    }


    // API ENDPOINT: GET /api/user/my-team
    // CALLED FROM FRONTEND: getMyTeam() function
    // PURPOSE: Gets all team members under the logged-in manager.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("my-team")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TeamMemberDto>>>> GetMyTeam()
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var employees = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);

        var teamMembers = employees.Select(e => new TeamMemberDto
        {
            UserId = e.UserId,
            Name = e.Name
        });

        return Ok(ApiResponseDto<IEnumerable<TeamMemberDto>>.SuccessResponse(teamMembers));
    }


    // API ENDPOINT: GET /api/user/manager-dashboard/{managerId}
    // CALLED FROM FRONTEND: getManagerDashboard() function
    // PURPOSE: Gets manager dashboard stats.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("manager-dashboard/{managerId}")]
    public async Task<IActionResult> GetManagerDashboard(Guid managerId)
    {
        // 1) Fetch team count using async count
        var teamCount = await _unitOfWork.Users.GetEmployeesCountByManagerIdAsync(managerId);

        // 2) Fetch team members list (sequential)
        var teamMembers = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);
        var teamMemberIds = teamMembers.Select(u => u.UserId).ToList();

        // 3) Fetch total team hours for today using repository async Sum (sequential)
        // Use DateTime.Today.Date to match only the date portion
        var today = DateTime.Today.Date;
        decimal teamHoursToday = 0m;
        if (teamMemberIds.Any())
        {
            teamHoursToday = await _unitOfWork.TimeLogs.GetTotalHoursByUsersForDateAsync(teamMemberIds, today);
        }

        // 4) Fetch active tasks count for the team using repository async Count (sequential)
        var activeTasks = 0;
        if (teamMemberIds.Any())
        {
            activeTasks = await _unitOfWork.Tasks.GetActiveTasksCountForUsersAsync(teamMemberIds);
        }

        var payload = new
        {
            teamCount = teamCount,
            teamHoursToday = teamHoursToday,
            activeTasks = activeTasks
        };

        return Ok(new { success = true, data = payload });
    }


    // API ENDPOINT: PATCH /api/user/{userId}/deactivate
    // CALLED FROM FRONTEND: deactivateUser() function
    // PURPOSE: Deactivates a user account.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{userId}/deactivate")]
    public async Task<ActionResult<ApiResponseDto<bool>>> DeactivateUser(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponseDto<bool>.ErrorResponse("User not found"));

        user.Status = "Inactive";
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "User deactivated"));
    }


    // API ENDPOINT: PATCH /api/user/{userId}/activate
    // CALLED FROM FRONTEND: activateUser() function
    // PURPOSE: Reactivates a user account.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpPatch("{userId}/activate")]
    public async Task<ActionResult<ApiResponseDto<bool>>> ActivateUser(Guid userId)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
            return NotFound(ApiResponseDto<bool>.ErrorResponse("User not found"));

        user.Status = "Active";
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Ok(ApiResponseDto<bool>.SuccessResponse(true, "User activated"));
    }

    // ==================== ORGANIZATION ANALYTICS ENDPOINTS ====================


    // API ENDPOINT: GET /api/user/active
    // CALLED FROM FRONTEND: getActiveUsers() function
    // PURPOSE: Gets list of currently active/punched-in employees.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserInfoDto>>>> GetActiveUsers()
    {
        var users = await _unitOfWork.Users.GetPunchedInUsersAsync();
        var userInfos = users.Select(u => new UserInfoDto
        {
            Id = u.UserId.ToString(),
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            Department = u.Department ?? string.Empty,
            IsActive = u.Status == "Active",
            LastLoginAt = u.UpdatedAt,
            PunchedInAt = null // Can be enhanced if you track punch-in separately
        });

        return Ok(ApiResponseDto<IEnumerable<UserInfoDto>>.SuccessResponse(userInfos));
    }

  
    // API ENDPOINT: GET /api/user/by-role/{role}
    // CALLED FROM FRONTEND: getUsersByRole() function
    // PURPOSE: Gets users filtered by role.
    // FLOW: Controller → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("by-role/{role}")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<UserInfoDto>>>> GetUsersByRole(
        string role,
        [FromQuery] bool? isActive)
    {
        if (string.IsNullOrWhiteSpace(role) || 
            !new[] { "Employee", "Manager", "Admin" }.Contains(role))
        {
            return BadRequest(ApiResponseDto<IEnumerable<UserInfoDto>>.ErrorResponse(
                "Invalid role. Use 'Employee', 'Manager', or 'Admin'"));
        }

        var users = await _unitOfWork.Users.GetUsersByRoleAsync(role);

        if (isActive.HasValue)
        {
            users = users.Where(u => (u.Status == "Active") == isActive.Value);
        }

        var userInfos = users.Select(u => new UserInfoDto
        {
            Id = u.UserId.ToString(),
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            Department = u.Department ?? string.Empty,
            IsActive = u.Status == "Active",
            LastLoginAt = u.UpdatedAt,
            PunchedInAt = null
        });

        return Ok(ApiResponseDto<IEnumerable<UserInfoDto>>.SuccessResponse(userInfos));
    }

    private static UserDto MapToDto(User u)
    {
        return new UserDto
        {
            UserId = u.UserId,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            Department = u.Department ?? string.Empty,
            Status = u.Status,
            ManagerId = u.ManagerId,
            ManagerName = u.Manager?.Name,
            AssignedEmployeeIds = u.AssignedEmployees?.Select(e => e.UserId).ToList() ?? new List<Guid>()
        };
    }
}
