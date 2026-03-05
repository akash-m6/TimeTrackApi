using TimeTrack.API.DTOs.Break;
using TimeTrack.API.Models;
using TimeTrack.API.Repository.IRepository;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Service;

// SERVICE: BreakService
// PURPOSE: Contains business logic for user break operations.
public class BreakService : IBreakService
{
    private readonly IUnitOfWork _unitOfWork;

    public BreakService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // METHOD: StartBreakAsync
    // PURPOSE: Starts a new break for the user.
    public async Task<BreakResponseDto> StartBreakAsync(Guid userId, CreateBreakDto dto)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(dto.TimeLogId);

        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only start breaks for your own time logs");

        var activeBreak = await _unitOfWork.Breaks.GetActiveBreakForTimeLogAsync(dto.TimeLogId);
        if (activeBreak != null)
            throw new InvalidOperationException("An active break already exists for this time log");

        if (!TimeSpan.TryParse(dto.StartTime, out var startTime))
            throw new ArgumentException("Invalid start time format");

        var breakEntity = new Break
        {
            TimeLogId = dto.TimeLogId,
            Activity = dto.Activity,
            StartTime = startTime,
            EndTime = null,
            Duration = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Breaks.AddAsync(breakEntity);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(breakEntity);
    }

    // METHOD: EndBreakAsync
    // PURPOSE: Ends an active break for the user.
    public async Task<BreakResponseDto> EndBreakAsync(Guid breakId, Guid userId, EndBreakDto dto)
    {
        var breakEntity = await _unitOfWork.Breaks.GetByIdAsync(breakId);

        if (breakEntity == null)
            throw new KeyNotFoundException("Break not found");

        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(breakEntity.TimeLogId);
        if (timeLog == null || timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only end your own breaks");

        if (breakEntity.EndTime != null)
            throw new InvalidOperationException("Break has already been ended");

        if (!TimeSpan.TryParse(dto.EndTime, out var endTime))
            throw new ArgumentException("Invalid end time format");

        if (endTime <= breakEntity.StartTime)
            throw new ArgumentException("End time must be after start time");

        var durationMinutes = (int)(endTime - breakEntity.StartTime).TotalMinutes;

        breakEntity.EndTime = endTime;
        breakEntity.Duration = durationMinutes;
        breakEntity.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Breaks.Update(breakEntity);

        timeLog.BreakDuration += durationMinutes;

        var activities = string.IsNullOrEmpty(timeLog.Activity)
            ? new List<string>()
            : timeLog.Activity.Split(',').Select(a => a.Trim()).ToList();

        if (!activities.Contains(breakEntity.Activity))
        {
            activities.Add(breakEntity.Activity);
            timeLog.Activity = string.Join(", ", activities);
        }

        _unitOfWork.TimeLogs.Update(timeLog);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(breakEntity);
    }

    // METHOD: GetBreaksForTimeLogAsync
    // PURPOSE: Retrieves all breaks for a specific time log.
    public async Task<IEnumerable<BreakResponseDto>> GetBreaksForTimeLogAsync(Guid timeLogId, Guid userId)
    {
        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(timeLogId);

        if (timeLog == null)
            throw new KeyNotFoundException("Time log not found");

        if (timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only view breaks for your own time logs");

        var breaks = await _unitOfWork.Breaks.GetBreaksByTimeLogIdAsync(timeLogId);

        return breaks.Select(MapToDto);
    }

    // METHOD: GetActiveBreakForUserAsync
    // PURPOSE: Retrieves the active break for a user.
    public async Task<BreakResponseDto?> GetActiveBreakForUserAsync(Guid userId)
    {
        var activeBreak = await _unitOfWork.Breaks.GetActiveBreakForUserAsync(userId);

        if (activeBreak == null)
            return null;

        return MapToDto(activeBreak);
    }

    // METHOD: DeleteBreakAsync
    // PURPOSE: Deletes a break for the user.
    public async Task<bool> DeleteBreakAsync(Guid breakId, Guid userId)
    {
        var breakEntity = await _unitOfWork.Breaks.GetByIdAsync(breakId);

        if (breakEntity == null)
            return false;

        var timeLog = await _unitOfWork.TimeLogs.GetByIdAsync(breakEntity.TimeLogId);
        if (timeLog == null || timeLog.UserId != userId)
            throw new UnauthorizedAccessException("You can only delete your own breaks");

        if (breakEntity.Duration.HasValue)
        {
            timeLog.BreakDuration -= breakEntity.Duration.Value;

            var activities = timeLog.Activity?.Split(',').Select(a => a.Trim()).ToList() ?? new List<string>();
            activities.Remove(breakEntity.Activity);
            timeLog.Activity = activities.Any() ? string.Join(", ", activities) : null;

            _unitOfWork.TimeLogs.Update(timeLog);
        }

        _unitOfWork.Breaks.Delete(breakEntity);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    // METHOD: MapToDto
    // PURPOSE: Maps Break entity to BreakResponseDto.
    private BreakResponseDto MapToDto(Break breakEntity)
    {
        return new BreakResponseDto
        {
            BreakId = breakEntity.BreakId,
            TimeLogId = breakEntity.TimeLogId,
            Activity = breakEntity.Activity,
            StartTime = breakEntity.StartTime.ToString(@"hh\:mm\:ss"),
            EndTime = breakEntity.EndTime?.ToString(@"hh\:mm\:ss"),
            Duration = breakEntity.Duration,
            CreatedAt = breakEntity.CreatedAt
        };
    }
}
