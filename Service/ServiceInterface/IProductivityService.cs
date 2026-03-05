using System;
using System.Threading.Tasks;
using TimeTrack.API.DTOs.Productivity;

// SERVICE INTERFACE: IProductivityService
// PURPOSE: Defines contract for productivity-related business logic.
// SERVICE INTERFACE: IProductivityService
// PURPOSE: Defines contract for productivity-related business logic.
// SERVICE INTERFACE: IProductivityService
// PURPOSE: Defines contract for productivity-related business logic.
public interface IProductivityService
{
    Task<ProductivityResponseDto> GetProductivityAsync(Guid userId);
    Task<ProductivityReportDto> GenerateUserReportAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<ProductivityReportDto> GenerateDepartmentReportAsync(string department, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateEfficiencyScoreAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateTaskCompletionRateAsync(Guid userId, DateTime startDate, DateTime endDate);
}
