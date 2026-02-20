using TimeTrack.API.DTOs.Productivity;
using TimeTrack.API.Repository.IRepository;
using TimeTrack.API.Models;

namespace TimeTrack.API.Service;

public class ProductivityAnalyticsService : IProductivityAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductivityAnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductivityReportDto> GenerateUserReportAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        var timeLogs = await _unitOfWork.TimeLogs.GetLogsByDateRangeAsync(userId, startDate, endDate);
        var totalHours = timeLogs.Sum(log => log.TotalHours);

        var userTasks = await _unitOfWork.Tasks.GetTasksByAssignedUserAsync(userId);
        var relevantTasks = userTasks.Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate).ToList();
        var completedTasks = relevantTasks.Count(t => t.Status == "Completed");

        var completionRate = await CalculateTaskCompletionRateAsync(userId, startDate, endDate);
        var efficiencyScore = await CalculateEfficiencyScoreAsync(userId, startDate, endDate);

        var dailyBreakdown = BuildDailyProductivityBreakdown(timeLogs, startDate, endDate);

        var avgCompletionTime = completedTasks > 0
            ? relevantTasks.Where(t => t.Status == "Completed" && t.CompletedDate.HasValue && t.CompletedDate.Value != default)
                          .Average(t => (t.CompletedDate!.Value - t.CreatedDate).TotalHours)
            : 0;

        return new ProductivityReportDto
        {
            ReportScope = "User",
            TargetName = user.Name,
            StartDate = startDate,
            EndDate = endDate,
            TotalHoursLogged = totalHours,
            TotalTasksAssigned = relevantTasks.Count,
            TasksCompleted = completedTasks,
            TaskCompletionRate = completionRate,
            AverageTaskCompletionTime = (decimal)avgCompletionTime,
            EfficiencyScore = efficiencyScore,
            DailyBreakdown = dailyBreakdown
        };
    }

    public async Task<ProductivityReportDto> GenerateDepartmentReportAsync(string department, DateTime startDate, DateTime endDate)
    {
        var departmentUsers = await _unitOfWork.Users.GetUsersByDepartmentAsync(department);
        var userIds = departmentUsers.Select(u => u.UserId).ToList();

        if (!userIds.Any())
        {
            throw new InvalidOperationException($"No active users found in department: {department}");
        }

        decimal totalHours = 0;
        int totalTasks = 0;
        int completedTasks = 0;

        foreach (var userId in userIds)
        {
            var userHours = await _unitOfWork.TimeLogs.GetTotalHoursByUserAsync(userId, startDate, endDate);
            totalHours += userHours;

            var userTasks = await _unitOfWork.Tasks.GetTasksByAssignedUserAsync(userId);
            var relevantTasks = userTasks.Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate).ToList();
            
            totalTasks += relevantTasks.Count;
            completedTasks += relevantTasks.Count(t => t.Status == "Completed");
        }

        var completionRate = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;
        var avgEfficiency = 0m;

        foreach (var userId in userIds)
        {
            avgEfficiency += await CalculateEfficiencyScoreAsync(userId, startDate, endDate);
        }
        avgEfficiency = userIds.Count > 0 ? avgEfficiency / userIds.Count : 0;

        return new ProductivityReportDto
        {
            ReportScope = "Department",
            TargetName = department,
            StartDate = startDate,
            EndDate = endDate,
            TotalHoursLogged = totalHours,
            TotalTasksAssigned = totalTasks,
            TasksCompleted = completedTasks,
            TaskCompletionRate = completionRate,
            AverageTaskCompletionTime = 0, // Can be enhanced with detailed tracking
            EfficiencyScore = avgEfficiency,
            DailyBreakdown = new List<DailyProductivityDto>()
        };
    }

    public async Task<decimal> CalculateEfficiencyScoreAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var totalHoursLogged = await _unitOfWork.TimeLogs.GetTotalHoursByUserAsync(userId, startDate, endDate);
        var totalTaskHours = await _unitOfWork.TaskTimes.GetTotalHoursForUserAsync(userId, startDate, endDate);

        if (totalHoursLogged == 0)
        {
            return 0;
        }

        // Calculate task-focused time percentage
        var taskFocusRatio = totalTaskHours / totalHoursLogged;

        // Get completion rate
        var completionRate = await CalculateTaskCompletionRateAsync(userId, startDate, endDate);

        // TimeTrack Efficiency Formula: 
        // (Task-focused Time Ratio * 0.6) + (Completion Rate * 0.4)
        var efficiencyScore = (taskFocusRatio * 60) + (completionRate * 0.4m);

        return Math.Min(100, Math.Round(efficiencyScore, 2));
    }

    public async Task<decimal> CalculateTaskCompletionRateAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var userTasks = await _unitOfWork.Tasks.GetTasksByAssignedUserAsync(userId);
        var relevantTasks = userTasks.Where(t => t.CreatedDate >= startDate && t.CreatedDate <= endDate).ToList();

        if (!relevantTasks.Any())
        {
            return 0;
        }

        var completedCount = relevantTasks.Count(t => t.Status == "Completed");
        return Math.Round((decimal)completedCount / relevantTasks.Count * 100, 2);
    }

    private List<DailyProductivityDto> BuildDailyProductivityBreakdown(
        IEnumerable<TimeLog> timeLogs, 
        DateTime startDate, 
        DateTime endDate)
    {
        var breakdown = new List<DailyProductivityDto>();

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayLogs = timeLogs.Where(log => log.Date.Date == date).ToList();
            
            breakdown.Add(new DailyProductivityDto
            {
                Date = date,
                HoursLogged = dayLogs.Sum(log => log.TotalHours),
                TasksWorkedOn = dayLogs.Count
            });
        }

        return breakdown;
    }
}