using TimeTrack.API.DTOs.Task;

namespace TimeTrack.API.Service;

public interface ITaskManagementService
{
    Task<TaskResponseDto> CreateTaskAsync(int creatorId, CreateTaskDto dto);
    Task<TaskResponseDto> UpdateTaskAsync(int taskId, CreateTaskDto dto);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<TaskResponseDto> GetTaskByIdAsync(int taskId);
    Task<IEnumerable<TaskResponseDto>> GetUserTasksAsync(int userId);
    Task<IEnumerable<TaskResponseDto>> GetCreatedTasksAsync(int creatorId);
    Task<bool> UpdateTaskStatusAsync(int taskId, string status);
    Task<bool> LogTaskTimeAsync(int userId, LogTaskTimeDto dto);
    Task<IEnumerable<TaskResponseDto>> GetOverdueTasksAsync();

    // NEW: Task workflow methods
    Task<TaskResponseDto> StartTaskAsync(int taskId, int userId);
    Task<TaskResponseDto> CompleteTaskAsync(int taskId, int userId);
    Task<TaskResponseDto> ApproveTaskAsync(int taskId, int managerId);
    Task<TaskResponseDto> RejectTaskAsync(int taskId, int managerId, string reason);
    Task<IEnumerable<TaskResponseDto>> GetTasksPendingApprovalAsync(int managerId);
}