using TimeTrack.API.DTOs.Productivity;

namespace TimeTrack.API.Service;

public interface IProductivityAnalyticsService
{
    Task<ProductivityReportDto> GenerateUserReportAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<ProductivityReportDto> GenerateDepartmentReportAsync(string department, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateEfficiencyScoreAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateTaskCompletionRateAsync(Guid userId, DateTime startDate, DateTime endDate);
}