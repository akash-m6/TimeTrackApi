using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TimeTrack.API.DTOs.Common;
using TimeTrack.API.DTOs.Task;
using TimeTrack.API.Service;

namespace TimeTrack.API.Controllers;

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

    /// <summary>
    /// Creates a new task and assigns it to an employee
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> CreateTask([FromBody] CreateTaskDto dto)
    {
        var creatorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.CreateTaskAsync(creatorId, dto);
        return CreatedAtAction(nameof(GetTaskById), new { taskId = result.TaskId }, 
            ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task created and assigned successfully"));
    }

    /// <summary>
    /// Updates task details (managers/admins only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> UpdateTask([FromRoute] int id, [FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.UpdateTaskAsync(id, dto);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task updated successfully"));
    }

    /// <summary>
    /// Deletes a task (managers/admins only, cannot delete completed tasks)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask([FromRoute] int id)
    {
        var result = await _taskService.DeleteTaskAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Retrieves task details by ID
    /// </summary>
    [HttpGet("{taskId}")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> GetTaskById(int taskId)
    {
        var result = await _taskService.GetTaskByIdAsync(taskId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets all tasks assigned to the current user
    /// </summary>
    [HttpGet("my-tasks")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetMyTasks()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetUserTasksAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Gets all tasks created by the current user (manager view)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("created-by-me")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetCreatedTasks()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetCreatedTasksAsync(userId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// START a task - Changes status from Pending to InProgress
    /// </summary>
    [HttpPatch("{taskId}/start")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> StartTask(int taskId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.StartTaskAsync(taskId, userId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task started successfully"));
    }

    /// <summary>
    /// COMPLETE a task - Changes status from InProgress to Completed (pending approval)
    /// </summary>
    [HttpPatch("{taskId}/complete")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> CompleteTask(int taskId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.CompleteTaskAsync(taskId, userId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task completed. Awaiting manager approval."));
    }

    /// <summary>
    /// APPROVE a completed task (Manager/Admin only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPatch("{taskId}/approve")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> ApproveTask(int taskId)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.ApproveTaskAsync(taskId, managerId);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task approved successfully"));
    }

    /// <summary>
    /// REJECT a completed task and send back to InProgress (Manager/Admin only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpPatch("{taskId}/reject")]
    public async Task<ActionResult<ApiResponseDto<TaskResponseDto>>> RejectTask(int taskId, [FromBody] RejectTaskRequest request)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.RejectTaskAsync(taskId, managerId, request.Reason);
        return Ok(ApiResponseDto<TaskResponseDto>.SuccessResponse(result, "Task rejected and sent back to employee"));
    }

    /// <summary>
    /// Gets tasks pending approval (Manager/Admin only)
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("pending-approval")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetTasksPendingApproval()
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.GetTasksPendingApprovalAsync(managerId);
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Logs time spent on a specific task by the assigned employee
    /// </summary>
    [HttpPost("log-time")]
    public async Task<ActionResult<ApiResponseDto<bool>>> LogTaskTime([FromBody] LogTaskTimeDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _taskService.LogTaskTimeAsync(userId, dto);
        return Ok(ApiResponseDto<bool>.SuccessResponse(result, "Task time logged successfully"));
    }

    /// <summary>
    /// Gets all overdue tasks for managers to track
    /// </summary>
    [Authorize(Policy = "ManagerOrAdmin")]
    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<TaskResponseDto>>>> GetOverdueTasks()
    {
        var result = await _taskService.GetOverdueTasksAsync();
        return Ok(ApiResponseDto<IEnumerable<TaskResponseDto>>.SuccessResponse(result));
    }
}

public record UpdateTaskStatusRequest(string Status);
public record RejectTaskRequest(string Reason);