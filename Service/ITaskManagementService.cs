using TimeTrack.API.DTOs.Task;

namespace TimeTrack.API.Service;

public interface ITaskManagementService
{
    Task<TaskResponseDto> CreateTaskAsync(Guid creatorId, CreateTaskDto dto);
    Task<TaskResponseDto> UpdateTaskAsync(Guid taskId, CreateTaskDto dto);
    Task<bool> DeleteTaskAsync(Guid taskId);
    Task<TaskResponseDto> GetTaskByIdAsync(Guid taskId);
    Task<IEnumerable<TaskResponseDto>> GetUserTasksAsync(Guid userId);
    Task<IEnumerable<TaskResponseDto>> GetCreatedTasksAsync(Guid creatorId);
    Task<bool> UpdateTaskStatusAsync(Guid taskId, string status);
    Task<bool> LogTaskTimeAsync(Guid userId, LogTaskTimeDto dto);
    Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync();

    // NEW: Task workflow methods
    Task<TaskResponseDto> StartTaskAsync(Guid taskId, Guid userId);
    Task<TaskResponseDto> CompleteTaskAsync(Guid taskId, Guid userId);
    Task<TaskResponseDto> ApproveTaskAsync(Guid taskId, Guid managerId);
    Task<TaskResponseDto> RejectTaskAsync(Guid taskId, Guid managerId, string reason);
    Task<IEnumerable<TaskResponseDto>> GetTasksPendingApprovalAsync(Guid managerId);
}