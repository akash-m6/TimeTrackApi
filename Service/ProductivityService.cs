using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TimeTrack.API.Data;
using TimeTrack.API.DTOs.Productivity;

namespace TimeTrack.API.Service
{
    public class ProductivityService : IProductivityService
    {
        private readonly TimeTrackDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ProductivityService> _logger;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public ProductivityService(TimeTrackDbContext db, IMemoryCache cache, ILogger<ProductivityService> logger)
        {
            _db = db;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ProductivityResponseDto> GetProductivityAsync(Guid userId)
        {
            var cacheKey = $"productivity:{userId}";
            if (_cache.TryGetValue(cacheKey, out ProductivityResponseDto cached))
            {
                _logger.LogInformation("Productivity cache hit for user {UserId}", userId);
                return cached;
            }

            _logger.LogInformation("Calculating productivity for user {UserId}", userId);

            // last 7 days: 6 days ago -> today (7 entries)
            var today = DateTime.UtcNow.Date;
            var startDate = today.AddDays(-6);

            try
            {
                var timeLogSums = await _db.TimeLogs
                    .Where(tl => tl.UserId == userId && tl.Date >= startDate && tl.Date <= today)
                    .GroupBy(tl => tl.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(x => x.TotalHours) })
                    .ToListAsync();

                var dailyHours = new decimal[7];
                for (int i = 0; i < 7; i++)
                {
                    var day = startDate.AddDays(i);
                    var entry = timeLogSums.FirstOrDefault(x => x.Date == day);
                    dailyHours[i] = entry?.Total ?? 0m;
                }

                var totalHoursLogged = dailyHours.Sum();
                var daysWithLogs = dailyHours.Count(h => h > 0m);
                var weeklyAverage = daysWithLogs > 0 ? Math.Round(totalHoursLogged / daysWithLogs, 2) : 0m;

                var taskStatuses = await _db.Tasks
                    .Where(t => t.AssignedToUserId == userId)
                    .Select(t => t.Status)
                    .ToListAsync();

                var totalTasks = taskStatuses.Count;
                var completed = 0;
                var inProgress = 0;
                var pending = 0;

                foreach (var status in taskStatuses)
                {
                    if (string.IsNullOrWhiteSpace(status)) continue;
                    var norm = status.Replace(" ", "").Replace("-", "").ToLowerInvariant();
                    if (norm == "completed" || norm == "complete") completed++;
                    else if (norm == "inprogress") inProgress++;
                    else if (norm == "pending") pending++;
                }

                var taskCompletionRate = totalTasks > 0 ? (int)Math.Round((double)completed * 100.0 / totalTasks) : 0;
                var efficiencyScore = totalTasks > 0 ? (int)Math.Round((double)(completed + inProgress) * 100.0 / totalTasks) : 0;

                var result = new ProductivityResponseDto
                {
                    TotalHoursLogged = Math.Round(totalHoursLogged, 2),
                    TaskCompletionRate = Math.Clamp(taskCompletionRate, 0, 100),
                    EfficiencyScore = Math.Clamp(efficiencyScore, 0, 100),
                    CompletedTasks = completed,
                    TotalTasks = totalTasks,
                    InProgressTasks = inProgress,
                    PendingTasks = pending,
                    WeeklyAverage = weeklyAverage,
                    DailyHours = dailyHours,
                    TaskDistribution = new TaskDistributionDto
                    {
                        Completed = completed,
                        InProgress = inProgress,
                        Pending = pending
                    }
                };

                _cache.Set(cacheKey, result, CacheDuration);
                _logger.LogInformation("Productivity calculated and cached for user {UserId}", userId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating productivity for user {UserId}", userId);
                throw;
            }
        }

        public Task<ProductivityReportDto> GenerateUserReportAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<ProductivityReportDto> GenerateDepartmentReportAsync(string department, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> CalculateEfficiencyScoreAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public Task<decimal> CalculateTaskCompletionRateAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }
    }
}