using TimeTrack.API.DTOs.Break;

namespace TimeTrack.API.Service.ServiceInterface;

public interface IBreakService
{
    Task<BreakResponseDto> StartBreakAsync(Guid userId, CreateBreakDto dto);
    Task<BreakResponseDto> EndBreakAsync(Guid breakId, Guid userId, EndBreakDto dto);
    Task<IEnumerable<BreakResponseDto>> GetBreaksForTimeLogAsync(Guid timeLogId, Guid userId);
    Task<BreakResponseDto?> GetActiveBreakForUserAsync(Guid userId);
    Task<bool> DeleteBreakAsync(Guid breakId, Guid userId);
}
