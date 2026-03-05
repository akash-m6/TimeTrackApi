using TimeTrack.API.DTOs.TimeLog;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Service;

// SERVICE: TimeLoggingService
// PURPOSE: Contains business logic for time log operations.
public class TimeLoggingService : ITimeLoggingService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimeLoggingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // METHOD: CreateTimeLogAsync
    // PURPOSE: Creates a new time log for the user.
    public async Task<TimeLogResponseDto> CreateTimeLogAsync(Guid userId, CreateTimeLogDto dto)
    {
        var existingLog = await _unitOfWork.TimeLogs.GetLogByUserAndDateAsync(userId, dto.Date);
        if (existingLog != null)
        {
            throw new InvalidOperationException("Time log already exists for this date");
        }

        var timeLog = new TimeLog
        {
            UserId = userId,
            Date = dto.Date.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakDuration = dto.BreakDuration,
            TotalHours = dto.TotalHours,
            Activity = dto.Activity
        };

        await _unitOfWork.TimeLogs.AddAsync(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseDto(timeLog);
    }

    // METHOD: UpdateTimeLogAsync
    // PURPOSE: Updates an existing time log for the user.
    public async Task<TimeLogResponseDto> UpdateTimeLogAsync(Guid logId, Guid userId, CreateTimeLogDto dto)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);

        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only update your own time logs");

        timeLog.Date = dto.Date.Date;
        timeLog.StartTime = dto.StartTime;
        timeLog.EndTime = dto.EndTime;
        timeLog.BreakDuration = dto.BreakDuration;
        timeLog.TotalHours = dto.TotalHours;
        timeLog.Activity = dto.Activity;

        _unitOfWork.TimeLogs.Update(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseDto(timeLog);
    }

    // METHOD: DeleteTimeLogAsync
    // PURPOSE: Deletes a time log for the user.
    public async Task<bool> DeleteTimeLogAsync(Guid logId, Guid userId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);

        if (timeLog == null)
            return false;

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own time logs");

        _unitOfWork.TimeLogs.Delete(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // METHOD: GetTimeLogByIdAsync
    // PURPOSE: Retrieves a time log by its ID.
    public async Task<TimeLogResponseDto> GetTimeLogByIdAsync(Guid logId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);
        
        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        return await MapToResponseDto(timeLog);
    }

    // METHOD: GetUserTimeLogsAsync
    // PURPOSE: Retrieves all time logs for the current user.
    public async Task<IEnumerable<TimeLogResponseDto>> GetUserTimeLogsAsync(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        IEnumerable<TimeLog> logs;

        if (startDate.HasValue && endDate.HasValue)
        {
            logs = await _unitOfWork.TimeLogs.GetLogsByDateRangeAsync(userId, startDate.Value, endDate.Value);
        }
        else
        {
            logs = await _unitOfWork.TimeLogs.GetLogsByUserIdAsync(userId);
        }

        return logs.Select(log => new TimeLogResponseDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            UserName = log.User?.Name,
            Date = log.Date,
            StartTime = log.StartTime,
            EndTime = log.EndTime,
            BreakDuration = log.BreakDuration,
            TotalHours = log.TotalHours,
            Activity = log.Activity
        });
    }

    // METHOD: GetDepartmentTimeLogsAsync
    // PURPOSE: Retrieves all time logs for a department in a date range.
    public async Task<IEnumerable<TimeLogResponseDto>> GetDepartmentTimeLogsAsync(string department, DateTime startDate, DateTime endDate)
    {
        var logs = await _unitOfWork.TimeLogs.GetLogsByDepartmentAsync(department, startDate, endDate);

        return logs.Select(log => new TimeLogResponseDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            UserName = log.User?.Name,
            Date = log.Date,
            StartTime = log.StartTime,
            EndTime = log.EndTime,
            BreakDuration = log.BreakDuration,
            TotalHours = log.TotalHours,
            Activity = log.Activity
        });
    }

    // METHOD: ApproveTimeLogAsync
    // PURPOSE: Approves a time log (manager/admin only).
    public async Task<bool> ApproveTimeLogAsync(Guid logId, Guid managerId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);

        if (timeLog == null)
            return false;

        // IsApproved field has been removed from the model
        // This method can be used for other approval logic if needed
        return true;
    }

    // METHOD: CalculateTotalHoursAsync
    // PURPOSE: Calculates total hours logged by the user in a date range.
    public async Task<decimal> CalculateTotalHoursAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.TimeLogs.GetTotalHoursByUserAsync(userId, startDate, endDate);
    }

    // METHOD: GetTeamTimeLogsByManagerIdAsync
    // PURPOSE: Retrieves all time logs for a manager's team.
    public async Task<IEnumerable<TeamTimeLogDto>> GetTeamTimeLogsByManagerIdAsync(Guid managerId)
    {
        var employees = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);

        if (employees == null || !employees.Any())
            return Enumerable.Empty<TeamTimeLogDto>();

        var result = new List<TeamTimeLogDto>();

        foreach (var emp in employees)
        {
            var logs = await _unitOfWork.TimeLogs.GetLogsByUserIdAsync(emp.UserId);

            foreach (var log in logs)
            {
                var status = DetermineLogStatus(log);

                result.Add(new TeamTimeLogDto
                {
                    LogId = log.LogId,
                    EmployeeId = emp.UserId,
                    EmployeeName = emp.Name,
                    Date = log.Date,
                    StartTime = log.StartTime,
                    EndTime = log.EndTime,
                    BreakDuration = log.BreakDuration,
                    TotalHours = log.TotalHours,
                    Activity = log.Activity,
                    Status = status
                });
            }
        }

        return result.OrderByDescending(t => t.Date).ThenByDescending(t => t.StartTime);
    }

    // METHOD: DetermineLogStatus
    // PURPOSE: Determines the status of a time log.
    private string DetermineLogStatus(TimeLog log)
    {
        if (log.EndTime == TimeSpan.Zero || log.TotalHours == 0)
            return "In Progress";

        return "Completed";
    }

    // METHOD: GetTotalHoursByUsersForDateAsync
    // PURPOSE: Returns total hours logged by a list of users for a specific date.
    public async Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<Guid> userIds, DateTime date)
    {
        return await _unitOfWork.TimeLogs.GetTotalHoursByUsersForDateAsync(userIds, date);
    }

    // METHOD: MapToResponseDto
    // PURPOSE: Maps TimeLog entity to TimeLogResponseDto.
    private async Task<TimeLogResponseDto> MapToResponseDto(TimeLog timeLog)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(timeLog.UserId);

        return new TimeLogResponseDto
        {
            LogId = timeLog.LogId,
            UserId = timeLog.UserId,
            UserName = user?.Name,
            Date = timeLog.Date,
            StartTime = timeLog.StartTime,
            EndTime = timeLog.EndTime,
            BreakDuration = timeLog.BreakDuration,
            TotalHours = timeLog.TotalHours,
            Activity = timeLog.Activity
        };
    }

    // Organization Analytics Methods
    // METHOD: GetAllTimeLogsWithDetailsAsync
    // PURPOSE: Retrieves all time logs with details for analytics.
    public async Task<IEnumerable<TimeLogResponseDto>> GetAllTimeLogsWithDetailsAsync(
        DateTime? startDate, 
        DateTime? endDate, 
        string? department, 
        string? status)
    {
        var logs = await _unitOfWork.TimeLogs.GetAllTimeLogsWithDetailsAsync(
            startDate, 
            endDate, 
            department, 
            status);

        return logs.Select(log => new TimeLogResponseDto
        {
            LogId = log.LogId,
            UserId = log.UserId,
            UserName = log.User?.Name,
            Date = log.Date,
            StartTime = log.StartTime,
            EndTime = log.EndTime,
            BreakDuration = log.BreakDuration,
            TotalHours = log.TotalHours,
            Activity = log.Activity
        });
    }
}
