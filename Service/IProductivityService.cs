using System;
using System.Threading.Tasks;
using TimeTrack.API.DTOs.Productivity;

public interface IProductivityService
{
    Task<ProductivityResponseDto> GetProductivityAsync(Guid userId);

    // Add these missing method signatures to match controller usage
    Task<ProductivityReportDto> GenerateUserReportAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<ProductivityReportDto> GenerateDepartmentReportAsync(string department, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateEfficiencyScoreAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<decimal> CalculateTaskCompletionRateAsync(Guid userId, DateTime startDate, DateTime endDate);
}