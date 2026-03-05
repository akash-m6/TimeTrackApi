using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Task;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Controllers;

// CONTROLLER: TaskController
// PURPOSE: Handles all task-related API requests from frontend.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskManagementService _taskService;

    public TaskController(ITaskManagementService taskService)
    {
        _taskService = taskService;
    }


    // API ENDPOINT: POST /api/task
    // CALLED FROM FRONTEND: createTask() function
    // PURPOSE: Creates a new task and assigns it to an employee.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> CreateTask([FromBody] CreateTaskDto dto)
    {
        var creatorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.CreateTaskAsync(creatorId, dto);
        return CreatedAtAction(nameof(GetTaskById), new { taskId = result.TaskId }, 
            ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task created and assigned successfully"));
    }


    // API ENDPOINT: PUT /api/task/{id}
    // CALLED FROM FRONTEND: updateTask() function
    // PURPOSE: Updates task details.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> UpdateTask([FromRoute] Guid id, [FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.UpdateTaskAsync(id, dto);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task updated successfully"));
    }


    // API ENDPOINT: DELETE /api/task/{id}
    // CALLED FROM FRONTEND: deleteTask() function
    // PURPOSE: Deletes a task (cannot delete completed tasks).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
    {
        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }


    // API ENDPOINT: GET /api/task/{taskId}
    // CALLED FROM FRONTEND: getTaskById() function
    // PURPOSE: Retrieves task details by ID.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("{taskId}")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> GetTaskById(Guid taskId)
    {
        var result = await _taskService.GetTaskByIdAsync(taskId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result));
    }

    // API ENDPOINT: GET /api/task/my-tasks
    // CALLED FROM FRONTEND: getMyTasks() function
    // PURPOSE: Gets all tasks assigned to the current user.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpGet("my-tasks")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetMyTasks()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetUserTasksAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }


    // API ENDPOINT: GET /api/task/created-by-me
    // CALLED FROM FRONTEND: getCreatedTasks() function
    // PURPOSE: Gets all tasks created by the current user (manager view).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("created-by-me")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetCreatedTasks()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetCreatedTasksAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }


    // API ENDPOINT: PATCH /api/task/{taskId}/start
    // CALLED FROM FRONTEND: startTask() function
    // PURPOSE: Starts a task (changes status from Pending to InProgress).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPatch("{taskId}/start")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> StartTask(Guid taskId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.StartTaskAsync(taskId, userId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task started successfully"));
    }


    // API ENDPOINT: PATCH /api/task/{taskId}/complete
    // CALLED FROM FRONTEND: completeTask() function
    // PURPOSE: Completes a task (changes status from InProgress to Completed).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPatch("{taskId}/complete")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> CompleteTask(Guid taskId)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.CompleteTaskAsync(taskId, userId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task completed. Awaiting manager approval."));
    }

  
    // API ENDPOINT: PATCH /api/task/{taskId}/approve
    // CALLED FROM FRONTEND: approveTask() function
    // PURPOSE: Approves a completed task (Manager/Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPatch("{taskId}/approve")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> ApproveTask(Guid taskId)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.ApproveTaskAsync(taskId, managerId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task approved successfully"));
    }

 
    // API ENDPOINT: PATCH /api/task/{taskId}/reject
    // CALLED FROM FRONTEND: rejectTask() function
    // PURPOSE: Rejects a completed task and sends back to InProgress (Manager/Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPatch("{taskId}/reject")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> RejectTask(Guid taskId, [FromBody] RejectTaskRequest request)
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.RejectTaskAsync(taskId, managerId, request.Reason);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task rejected and sent back to employee"));
    }


    // API ENDPOINT: GET /api/task/pending-approval
    // CALLED FROM FRONTEND: getTasksPendingApproval() function
    // PURPOSE: Gets tasks pending approval (Manager/Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("pending-approval")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetTasksPendingApproval()
    {
        var managerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetTasksPendingApprovalAsync(managerId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }


    // API ENDPOINT: POST /api/task/log-time
    // CALLED FROM FRONTEND: logTaskTime() function
    // PURPOSE: Logs time spent on a specific task by the assigned employee.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [HttpPost("log-time")]
    public async Task<ActionResult<ApiResponseDto<bool>>> LogTaskTime([FromBody] LogTaskTimeDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.LogTaskTimeAsync(userId, dto);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Task time logged successfully"));
    }

  
    // API ENDPOINT: GET /api/task/overdue
    // CALLED FROM FRONTEND: getOverdueTasks() function
    // PURPOSE: Gets all overdue tasks for managers to track.
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetOverdueTasks()
    {
        var result = await _taskService.GetOverdueTasksAsync();
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }

    // ==================== ORGANIZATION ANALYTICS ENDPOINTS ====================

 
    // API ENDPOINT: GET /api/task/all
    // CALLED FROM FRONTEND: getAllTasks() function
    // PURPOSE: Gets all tasks with details for organization analytics (Admin only).
    // FLOW: Controller → Service → Repository → Database → Response to Frontend
    [Authorize(Policy = "AdminOnly")]
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetAllTasks(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? status,
        [FromQuery] string? department)
    {
        var result = await _taskService.GetAllTasksWithDetailsAsync(startDate, endDate, status, department);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }
}

public record UpdateTaskStatusRequest(string Status);
public record RejectTaskRequest(string Reason);
