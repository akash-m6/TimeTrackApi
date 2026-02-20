using TimeTrack.API.DTOs.TimeLog;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;

namespace TimeTrack.API.Service;

public class TimeLoggingService : ITimeLoggingService
{
    private readonly IUnitOfWork _unitOfWork;

    public TimeLoggingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TimeLogResponseDto> CreateTimeLogAsync(Guid userId, CreateTimeLogDto dto)
    {
        var existingLog = await _unitOfWork.TimeLogs.GetLogByUserAndDateAsync(userId, dto.Date);
        if (existingLog != null)
        {
            throw new InvalidOperationException("Time log already exists for this date");
        }

        var totalHours = CalculateTotalHours(dto.StartTime, dto.EndTime, dto.BreakDuration);

        var timeLog = new TimeLog
        {
            UserId = userId,
            Date = dto.Date.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            BreakDuration = dto.BreakDuration,
            TotalHours = totalHours,
            Notes = dto.Notes,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TimeLogs.AddAsync(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseDto(timeLog);
    }

    public async Task<TimeLogResponseDto> UpdateTimeLogAsync(Guid logId, Guid userId, CreateTimeLogDto dto)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);
        
        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only update your own time logs");

        if (timeLog.IsApproved)
            throw new InvalidOperationException("Cannot update approved time log");

        timeLog.Date = dto.Date.Date;
        timeLog.StartTime = dto.StartTime;
        timeLog.EndTime = dto.EndTime;
        timeLog.BreakDuration = dto.BreakDuration;
        timeLog.TotalHours = CalculateTotalHours(dto.StartTime, dto.EndTime, dto.BreakDuration);
        timeLog.Notes = dto.Notes;
        timeLog.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.TimeLogs.Update(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return await MapToResponseDto(timeLog);
    }

    public async Task<bool> DeleteTimeLogAsync(Guid logId, Guid userId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);
        
        if (timeLog == null)
            return false;

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own time logs");

        if (timeLog.IsApproved)
            throw new InvalidOperationException("Cannot delete approved time log");

        _unitOfWork.TimeLogs.Delete(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<TimeLogResponseDto> GetTimeLogByIdAsync(Guid logId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);
        
        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        return await MapToResponseDto(timeLog);
    }

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
            Notes = log.Notes,
            IsApproved = log.IsApproved,
            CreatedDate = log.CreatedAt
        });
    }

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
            Notes = log.Notes,
            IsApproved = log.IsApproved,
            CreatedDate = log.CreatedAt
        });
    }

    public async Task<bool> ApproveTimeLogAsync(Guid logId, Guid managerId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(logId);
        
        if (timeLog == null)
            return false;

        timeLog.IsApproved = true;
        timeLog.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.TimeLogs.Update(timeLog);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> CalculateTotalHoursAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _unitOfWork.TimeLogs.GetTotalHoursByUserAsync(userId, startDate, endDate);
    }

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
                result.Add(new TeamTimeLogDto
                {
                    EmployeeName = emp.Name,
                    Date = log.Date,
                    StartTime = log.StartTime,
                    EndTime = log.EndTime,
                    BreakDuration = log.BreakDuration,
                    TotalHours = log.TotalHours
                });
            }
        }

        return result;
    }

    public async Task<decimal> GetTotalHoursByUsersForDateAsync(IEnumerable<Guid> userIds, DateTime date)
    {
        return await _unitOfWork.TimeLogs.GetTotalHoursByUsersForDateAsync(userIds, date);
    }

    private decimal CalculateTotalHours(TimeSpan startTime, TimeSpan endTime, TimeSpan breakDuration)
    {
        var totalTime = endTime - startTime;
        var workTime = totalTime - breakDuration;
        return (decimal)workTime.TotalHours;
    }

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
            Notes = timeLog.Notes,
            IsApproved = timeLog.IsApproved,
            CreatedDate = timeLog.CreatedAt
        };
    }
}