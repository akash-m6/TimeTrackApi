using TimeTrack.API.DTOs.Analytics;
using TimeTrack.API.Repository.IRepository;
using TimeTrack.API.Service.ServiceInterface;

namespace TimeTrack.API.Service;

// SERVICE: AnalyticsService
// PURPOSE: Provides business logic for analytics and reporting features.
public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    // METHOD: GetTeamSummaryAsync
    // PURPOSE: Gets team summary analytics for dashboard cards.
    public async Task<TeamSummaryDto> GetTeamSummaryAsync(Guid managerId, DateTime? startDate, DateTime? endDate)
    {
        // Set default date range if not provided (last 30 days)
        var calculatedFrom = startDate ?? DateTime.UtcNow.AddDays(-30);
        var calculatedTo = endDate ?? DateTime.UtcNow;

        // Get manager's team members
        var teamMembers = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);
        var teamMemberIds = teamMembers.Select(m => m.UserId).ToList();
        var teamMemberCount = teamMemberIds.Count;

        // Get total team hours from TaskTime entries
        decimal totalTeamHours = 0;
        if (teamMemberIds.Any())
        {
            foreach (var memberId in teamMemberIds)
            {
                var memberHours = await _unitOfWork.TaskTimes.GetTotalHoursForUserAsync(memberId, calculatedFrom, calculatedTo);
                totalTeamHours += memberHours;
            }
        }

        // Calculate average hours per member
        var averageHoursPerMember = teamMemberCount > 0 ? totalTeamHours / teamMemberCount : 0;

        // Get tasks created by manager within date range
        var allTasks = await _unitOfWork.Tasks.GetTasksByCreatorAsync(managerId);
        var tasksInPeriod = allTasks.Where(t => t.CreatedDate >= calculatedFrom && t.CreatedDate <= calculatedTo).ToList();
        
        var totalTasksCount = tasksInPeriod.Count;
        var completedTasksCount = tasksInPeriod.Count(t => t.Status == "Completed" || t.IsApproved);
        var completionRate = totalTasksCount > 0 ? (int)((decimal)completedTasksCount / totalTasksCount * 100) : 0;

        return new TeamSummaryDto
        {
            TotalTeamHours = Math.Round(totalTeamHours, 2),
            AverageHoursPerMember = Math.Round(averageHoursPerMember, 2),
            CompletionRate = completionRate,
            CompletedTasksCount = completedTasksCount,
            TotalTasksCount = totalTasksCount,
            TeamMemberCount = teamMemberCount,
            CalculatedFrom = calculatedFrom,
            CalculatedTo = calculatedTo
        };
    }

    // METHOD: GetTeamHoursTrendAsync
    // PURPOSE: Gets team hours trend data grouped by day or week for line chart.
    public async Task<TeamHoursTrendDto> GetTeamHoursTrendAsync(Guid managerId, DateTime startDate, DateTime endDate, string groupBy)
    {
        // Get manager's team members
        var teamMembers = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);
        var teamMemberIds = teamMembers.Select(m => m.UserId).ToList();

        if (!teamMemberIds.Any())
        {
            return new TeamHoursTrendDto { TrendData = new List<TrendDataPoint>() };
        }

        // Get all TaskTime entries for team members in date range
        var allTaskTimes = new List<Models.TaskTime>();
        foreach (var memberId in teamMemberIds)
        {
            var memberTimes = await _unitOfWork.TaskTimes.GetTaskTimesByUserIdAsync(memberId, startDate, endDate);
            allTaskTimes.AddRange(memberTimes);
        }

        // Get all tasks created by manager
        var allTasks = await _unitOfWork.Tasks.GetTasksByCreatorAsync(managerId);
        var tasksInPeriod = allTasks.Where(t => t.CompletedDate.HasValue && 
                                                 t.CompletedDate.Value >= startDate && 
                                                 t.CompletedDate.Value <= endDate).ToList();

        // Group by day or week
        var trendData = new List<TrendDataPoint>();

        if (groupBy?.ToLower() == "week")
        {
            // Group by week
            var weeklyData = allTaskTimes
                .GroupBy(tt => GetWeekStart(tt.Date))
                .OrderBy(g => g.Key)
                .Select(g => new TrendDataPoint
                {
                    Date = g.Key,
                    TotalHours = g.Sum(tt => tt.HoursSpent),
                    TasksCompleted = tasksInPeriod.Count(t => t.CompletedDate.HasValue && 
                                                              GetWeekStart(t.CompletedDate.Value) == g.Key),
                    ActiveMembers = g.Select(tt => tt.UserId).Distinct().Count()
                });
            trendData = weeklyData.ToList();
        }
        else
        {
            // Group by day (default)
            var dailyData = allTaskTimes
                .GroupBy(tt => tt.Date.Date)
                .OrderBy(g => g.Key)
                .Select(g => new TrendDataPoint
                {
                    Date = g.Key,
                    TotalHours = g.Sum(tt => tt.HoursSpent),
                    TasksCompleted = tasksInPeriod.Count(t => t.CompletedDate.HasValue && 
                                                              t.CompletedDate.Value.Date == g.Key),
                    ActiveMembers = g.Select(tt => tt.UserId).Distinct().Count()
                });
            trendData = dailyData.ToList();
        }

        return new TeamHoursTrendDto
        {
            TrendData = trendData
        };
    }

    // METHOD: GetTeamMemberPerformanceAsync
    // PURPOSE: Gets individual performance metrics for each team member.
    public async Task<TeamMemberPerformanceDto> GetTeamMemberPerformanceAsync(Guid managerId, DateTime? startDate, DateTime? endDate)
    {
        // Set default date range
        var calculatedFrom = startDate ?? DateTime.UtcNow.AddDays(-30);
        var calculatedTo = endDate ?? DateTime.UtcNow;

        // Get manager's team members
        var teamMembers = await _unitOfWork.Users.GetEmployeesByManagerIdAsync(managerId);
        var memberPerformances = new List<MemberPerformance>();

        foreach (var member in teamMembers)
        {
            // Get total hours logged by this member
            var totalHours = await _unitOfWork.TaskTimes.GetTotalHoursForUserAsync(member.UserId, calculatedFrom, calculatedTo);

            // Get tasks assigned to this member
            var allTasks = await _unitOfWork.Tasks.GetTasksByAssignedUserAsync(member.UserId);
            var tasksInPeriod = allTasks.Where(t => t.CreatedDate >= calculatedFrom && t.CreatedDate <= calculatedTo).ToList();

            var tasksAssigned = tasksInPeriod.Count;
            var tasksCompleted = tasksInPeriod.Count(t => t.Status == "Completed" || t.IsApproved);
            var tasksInProgress = tasksInPeriod.Count(t => t.Status == "InProgress");
            var tasksPending = tasksInPeriod.Count(t => t.Status == "Pending");
            var overdueTasksCount = tasksInPeriod.Count(t => t.DueDate.HasValue && 
                                                              t.DueDate.Value < DateTime.UtcNow && 
                                                              t.Status != "Completed" && 
                                                              t.Status != "Approved");

            // Calculate efficiency score
            var efficiencyScore = tasksAssigned > 0 ? (decimal)tasksCompleted / tasksAssigned * 100 : 0;
            
            // Calculate average task completion time
            var completedTasksWithHours = tasksInPeriod.Where(t => t.Status == "Completed" || t.IsApproved).ToList();
            decimal averageTaskCompletionTime = 0;
            if (completedTasksWithHours.Any())
            {
                decimal totalCompletedHours = 0;
                foreach (var task in completedTasksWithHours)
                {
                    var taskHours = await _unitOfWork.TaskTimes.GetTotalHoursForTaskAsync(task.TaskId);
                    totalCompletedHours += taskHours;
                }
                averageTaskCompletionTime = totalCompletedHours / completedTasksWithHours.Count;
            }

            // Determine performance status
            string performanceStatus;
            if (efficiencyScore >= 90)
                performanceStatus = "Excellent";
            else if (efficiencyScore >= 70)
                performanceStatus = "Good";
            else
                performanceStatus = "Needs Attention";

            memberPerformances.Add(new MemberPerformance
            {
                UserId = member.UserId,
                Name = member.Name,
                Email = member.Email,
                TotalHours = Math.Round(totalHours, 2),
                TasksAssigned = tasksAssigned,
                TasksCompleted = tasksCompleted,
                TasksInProgress = tasksInProgress,
                TasksPending = tasksPending,
                EfficiencyScore = Math.Round(efficiencyScore, 2),
                PerformanceStatus = performanceStatus,
                AverageTaskCompletionTime = Math.Round(averageTaskCompletionTime, 2),
                OverdueTasksCount = overdueTasksCount
            });
        }

        return new TeamMemberPerformanceDto
        {
            Members = memberPerformances.OrderByDescending(m => m.EfficiencyScore).ToList()
        };
    }


    // METHOD: GetTaskCompletionBreakdownAsync
    // PURPOSE: Gets task count breakdown by status for doughnut chart.
    public async Task<TaskCompletionBreakdownDto> GetTaskCompletionBreakdownAsync(Guid managerId, DateTime? startDate, DateTime? endDate)
    {
        // Set default date range
        var calculatedFrom = startDate ?? DateTime.UtcNow.AddDays(-30);
        var calculatedTo = endDate ?? DateTime.UtcNow;

        // Get tasks created by manager
        var allTasks = await _unitOfWork.Tasks.GetTasksByCreatorAsync(managerId);
        var tasksInPeriod = allTasks.Where(t => t.CreatedDate >= calculatedFrom && t.CreatedDate <= calculatedTo).ToList();

        var totalCount = tasksInPeriod.Count;
        var completedCount = tasksInPeriod.Count(t => t.Status == "Completed" || t.Status == "Approved");
        var inProgressCount = tasksInPeriod.Count(t => t.Status == "InProgress");
        var pendingCount = tasksInPeriod.Count(t => t.Status == "Pending");
        var rejectedCount = tasksInPeriod.Count(t => t.IsRejected);
        var overdueCount = tasksInPeriod.Count(t => t.DueDate.HasValue && 
                                                     t.DueDate.Value < DateTime.UtcNow && 
                                                     t.Status != "Completed" && 
                                                     t.Status != "Approved");

        var completionPercentage = totalCount > 0 ? (decimal)completedCount / totalCount * 100 : 0;

        return new TaskCompletionBreakdownDto
        {
            CompletedCount = completedCount,
            InProgressCount = inProgressCount,
            PendingCount = pendingCount,
            RejectedCount = rejectedCount,
            OverdueCount = overdueCount,
            TotalCount = totalCount,
            CompletionPercentage = Math.Round(completionPercentage, 2)
        };
    }

  
    // METHOD: GetWeekStart
    // PURPOSE: Helper method to get the start of the week for a given date.
    private DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

 
    // METHOD: GetOrganizationSummaryAsync
    // PURPOSE: Gets organization-wide analytics summary for admin dashboard.
    public async Task<OrganizationAnalyticsResponse> GetOrganizationSummaryAsync(
        DateTime? startDate, 
        DateTime? endDate, 
        int? period)
    {
        // Determine date range
        DateTime calculatedEndDate = endDate ?? DateTime.UtcNow;
        DateTime calculatedStartDate;

        if (period.HasValue)
        {
            calculatedStartDate = calculatedEndDate.AddDays(-period.Value);
        }
        else
        {
            calculatedStartDate = startDate ?? calculatedEndDate.AddDays(-7);
        }

        // Get all active users
        var allUsers = await _unitOfWork.Users.GetActiveUsersAsync();
        var totalEmployees = allUsers.Count();

        // Get role distribution
        var employeeCount = await _unitOfWork.Users.GetUserCountByRoleAsync("Employee");
        var managerCount = await _unitOfWork.Users.GetUserCountByRoleAsync("Manager");
        var adminCount = await _unitOfWork.Users.GetUserCountByRoleAsync("Admin");

        // Get total hours logged (using TimeLogs)
        var totalHoursLogged = await _unitOfWork.TimeLogs.GetTotalHoursForOrganizationAsync(
            calculatedStartDate, 
            calculatedEndDate);

        // Calculate average hours per employee
        var avgHoursPerEmployee = totalEmployees > 0 ? totalHoursLogged / totalEmployees : 0;

        // Get active employees (currently punched in) - for simplicity, count active users with logs today
        var activeEmployees = await GetActiveEmployeesCountAsync();

        // Get task metrics
        var completedTasks = await _unitOfWork.Tasks.GetTaskCountByStatusAsync(
            "Completed", 
            calculatedStartDate, 
            calculatedEndDate);

        var inProgressTasks = await _unitOfWork.Tasks.GetTaskCountByStatusAsync(
            "InProgress", 
            calculatedStartDate, 
            calculatedEndDate);

        var pendingTasks = await _unitOfWork.Tasks.GetTaskCountByStatusAsync(
            "Pending", 
            calculatedStartDate, 
            calculatedEndDate);

        var totalTasks = completedTasks + inProgressTasks + pendingTasks;
        var taskCompletionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

        // Get department metrics
        var departments = await _unitOfWork.Users.GetAllDepartmentsAsync();
        var departmentMetrics = new List<DepartmentAnalyticsDto>();

        foreach (var dept in departments)
        {
            var deptAnalytics = await GetDepartmentAnalyticsAsync(dept, calculatedStartDate, calculatedEndDate);
            departmentMetrics.Add(deptAnalytics);
        }

        var avgEmployeesPerDepartment = departments.Any() 
            ? (decimal)totalEmployees / departments.Count 
            : 0;

        // Get hours trend data
        var hoursTrendData = await GetHoursTrendAsync(period ?? 7);

        // Determine period range string
        string periodRange = period.HasValue 
            ? $"Last {period.Value} days" 
            : $"{calculatedStartDate:MMM dd} - {calculatedEndDate:MMM dd}";

        return new OrganizationAnalyticsResponse
        {
            TotalHoursLogged = Math.Round(totalHoursLogged, 2),
            AvgHoursPerEmployee = Math.Round(avgHoursPerEmployee, 2),
            ActiveEmployees = activeEmployees,
            TotalEmployees = totalEmployees,
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            PendingTasks = pendingTasks,
            TaskCompletionPercentage = Math.Round(taskCompletionPercentage, 2),
            EmployeeCount = employeeCount,
            ManagerCount = managerCount,
            AdminCount = adminCount,
            DepartmentMetrics = departmentMetrics,
            AvgEmployeesPerDepartment = Math.Round(avgEmployeesPerDepartment, 2),
            HoursTrendData = hoursTrendData,
            ReportGeneratedAt = DateTime.UtcNow,
            PeriodRange = periodRange
        };
    }


    // METHOD: GetDepartmentAnalyticsAsync
    // PURPOSE: Gets detailed analytics for a specific department.
    public async Task<DepartmentAnalyticsDto> GetDepartmentAnalyticsAsync(
        string departmentName, 
        DateTime? startDate, 
        DateTime? endDate)
    {
        var calculatedStartDate = startDate ?? DateTime.UtcNow.AddDays(-7);
        var calculatedEndDate = endDate ?? DateTime.UtcNow;

        // Get employees in department
        var deptEmployees = await _unitOfWork.Users.GetUsersByDepartmentAsync(departmentName);
        var employeeIds = deptEmployees.Select(e => e.UserId).ToList();
        var employeeCount = employeeIds.Count;

        // Get total hours for department
        decimal totalHours = 0;
        foreach (var empId in employeeIds)
        {
            var empHours = await _unitOfWork.TimeLogs.GetTotalHoursByUserAsync(
                empId, 
                calculatedStartDate, 
                calculatedEndDate);
            totalHours += empHours;
        }

        var avgHoursPerEmployee = employeeCount > 0 ? totalHours / employeeCount : 0;

        // Get tasks for department
        var allTasks = await _unitOfWork.Tasks.GetAllTasksWithDetailsAsync(
            calculatedStartDate, 
            calculatedEndDate, 
            null, 
            departmentName);

        var completedTasks = allTasks.Count(t => t.Status == "Completed" || t.IsApproved);
        var inProgressTasks = allTasks.Count(t => t.Status == "InProgress");
        var pendingTasks = allTasks.Count(t => t.Status == "Pending");

        return new DepartmentAnalyticsDto
        {
            DepartmentName = departmentName,
            EmployeeCount = employeeCount,
            TotalHours = Math.Round(totalHours, 2),
            AvgHoursPerEmployee = Math.Round(avgHoursPerEmployee, 2),
            CompletedTasks = completedTasks,
            InProgressTasks = inProgressTasks,
            PendingTasks = pendingTasks,
            EmployeeIds = employeeIds.Select(id => id.ToString()).ToList()
        };
    }

    // METHOD: GetHoursTrendAsync
    // PURPOSE: Gets daily hours trend data for the chart.
    public async Task<List<DailyHoursDto>> GetHoursTrendAsync(int days)
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-days + 1);

        // Get daily aggregated hours
        var dailyHours = await _unitOfWork.TimeLogs.GetDailyHoursAggregateAsync(startDate, endDate);

        // Create trend data for all days (including days with no logs)
        var trendData = new List<DailyHoursDto>();
        for (int i = 0; i < days; i++)
        {
            var currentDate = startDate.AddDays(i);
            var totalHours = dailyHours.ContainsKey(currentDate) ? dailyHours[currentDate] : 0m;

            // Count active employees for the day (simplified - users who have logs)
            var activeCount = totalHours > 0 ? 1 : 0; // This is simplified

            trendData.Add(new DailyHoursDto
            {
                Date = currentDate,
                TotalHours = Math.Round(totalHours, 2),
                ActiveEmployees = activeCount,
                DateLabel = currentDate.ToString("MMM dd")
            });
        }

        return trendData;
    }

    // METHOD: GetActiveEmployeesCountAsync
    // PURPOSE: Helper method to get count of currently active employees.
    private async Task<int> GetActiveEmployeesCountAsync()
    {
        // Get users who have logged time today
        var today = DateTime.UtcNow.Date;
        var allUsers = await _unitOfWork.Users.GetActiveUsersAsync();

        int activeCount = 0;
        foreach (var user in allUsers)
        {
            var todayLog = await _unitOfWork.TimeLogs.GetLogByUserAndDateAsync(user.UserId, today);
            if (todayLog != null && todayLog.EndTime == null)
            {
                activeCount++;
            }
        }

        return activeCount;
    }
}

