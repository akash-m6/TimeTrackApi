using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Models;
using TaskAsync = System.Threading.Tasks.Task;

namespace TimeTrack.API.Data;

public static class DatabaseSeeder
{
    public static async TaskAsync SeedAsync(TimeTrackDbContext context)
    {
        // Seed admin user if not exists
        var admin = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@backend.com");
        
        if (admin == null)
        {
            admin = new User
            {
                Name = "System Administrator",
                Email = "admin@backend.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                Department = "IT",
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }

        // Fix existing tasks with null approval fields
        var now = DateTime.UtcNow;
        
        // Update Approved tasks missing ApprovedDate/ApprovedByUserId
        var approvedTasks = await context.Tasks
            .Where(t => t.Status == "Approved" && (t.ApprovedDate == null || t.ApprovedByUserId == null))
            .ToListAsync();

        foreach (var task in approvedTasks)
        {
            task.StartedDate ??= task.CreatedDate;
            task.CompletedDate ??= task.CreatedDate;
            task.IsApproved = true;
            task.ApprovedDate ??= now;
            task.ApprovedByUserId ??= task.CreatedByUserId;
        }

        // Update Completed tasks missing dates
        var completedTasks = await context.Tasks
            .Where(t => t.Status == "Completed" && (t.StartedDate == null || t.CompletedDate == null))
            .ToListAsync();

        foreach (var task in completedTasks)
        {
            task.StartedDate ??= task.CreatedDate;
            task.CompletedDate ??= now;
        }

        // Update InProgress tasks missing StartedDate
        var inProgressTasks = await context.Tasks
            .Where(t => t.Status == "InProgress" && t.StartedDate == null)
            .ToListAsync();

        foreach (var task in inProgressTasks)
        {
            task.StartedDate ??= task.CreatedDate;
        }

        if (approvedTasks.Any() || completedTasks.Any() || inProgressTasks.Any())
        {
            await context.SaveChangesAsync();
        }

        // Seed sample tasks if no tasks exist
        if (!await context.Tasks.AnyAsync())
        {
            var tasks = new List<TaskEntity>
            {
                new TaskEntity
                {
                    Title = "Setup Development Environment",
                    Description = "Configure IDE and install required tools",
                    AssignedToUserId = admin.UserId,
                    CreatedByUserId = admin.UserId,
                    EstimatedHours = 4,
                    Status = "Approved",
                    Priority = "High",
                    CreatedDate = now.AddDays(-7),
                    CreatedAt = now.AddDays(-7),
                    StartedDate = now.AddDays(-6),
                    CompletedDate = now.AddDays(-5),
                    CompletedAt = now.AddDays(-5),
                    IsApproved = true,
                    ApprovedDate = now.AddDays(-4),
                    ApprovedByUserId = admin.UserId
                },
                new TaskEntity
                {
                    Title = "Implement User Authentication",
                    Description = "Add JWT authentication to API",
                    AssignedToUserId = admin.UserId,
                    CreatedByUserId = admin.UserId,
                    EstimatedHours = 8,
                    Status = "Completed",
                    Priority = "High",
                    CreatedDate = now.AddDays(-5),
                    CreatedAt = now.AddDays(-5),
                    StartedDate = now.AddDays(-4),
                    CompletedDate = now.AddDays(-1),
                    CompletedAt = now.AddDays(-1),
                    IsApproved = false
                },
                new TaskEntity
                {
                    Title = "Create Task Management API",
                    Description = "Build CRUD endpoints for tasks",
                    AssignedToUserId = admin.UserId,
                    CreatedByUserId = admin.UserId,
                    EstimatedHours = 6,
                    Status = "InProgress",
                    Priority = "Medium",
                    CreatedDate = now.AddDays(-3),
                    CreatedAt = now.AddDays(-3),
                    StartedDate = now.AddDays(-2),
                    IsApproved = false
                },
                new TaskEntity
                {
                    Title = "Write Unit Tests",
                    Description = "Add unit tests for services",
                    AssignedToUserId = admin.UserId,
                    CreatedByUserId = admin.UserId,
                    EstimatedHours = 5,
                    Status = "Pending",
                    Priority = "Low",
                    DueDate = now.AddDays(7),
                    CreatedDate = now,
                    CreatedAt = now,
                    IsApproved = false
                }
            };

            context.Tasks.AddRange(tasks);
            await context.SaveChangesAsync();
        }
    }
}