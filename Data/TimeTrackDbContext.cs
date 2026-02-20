using Microsoft.EntityFrameworkCore;
using TimeTrack.API.Models;
using TaskModel = TimeTrack.API.Models.TaskEntity;

namespace TimeTrack.API.Data
{
    public class TimeTrackDbContext : DbContext
    {
        public TimeTrackDbContext(DbContextOptions<TimeTrackDbContext> options)
            : base(options)
        {
        }

        // ✅ Updated to use new Guid-based models
        public DbSet<User> Users { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }
        public DbSet<TaskTime> TaskTimes { get; set; }
        public DbSet<PendingRegistration> PendingRegistrations { get; set; }
        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);



            // ---------------- DECIMAL PRECISION ----------------
            // (Safer than HasColumnType for EF Core 7/8 style; keeps model authoritative)
            modelBuilder.Entity<TaskEntity>()
                .Property(t => t.EstimatedHours)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TimeLog>()
                .Property(tl => tl.TotalHours)
                .HasPrecision(5, 2);

            modelBuilder.Entity<TaskTime>()
                .Property(tt => tt.HoursSpent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Project>()
                .Property(p => p.Budget)
                .HasPrecision(18, 2);

            // ---------------- USER SELF-REFERENCE (Manager) ----------------
            // You already have User.ManagerId and [ForeignKey("ManagerId")] in model.
            // This maps the hierarchy and prevents cascade loops on user deletions.
            modelBuilder.Entity<User>()
                .HasOne(u => u.Manager)
                .WithMany(u => u.AssignedEmployees) // This matches your 'AssignedEmployees' collection
                .HasForeignKey(u => u.ManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------------- PROJECTS ↔ USERS (Manager) ----------------
            // Your Project model has ManagerUserId (FK) and a 'Manager' navigation (as seen in migration).
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Manager)
                .WithMany() // no 'ManagedProjects' collection on User, so omit right-side nav
                .HasForeignKey(p => p.ManagerUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------------- TASKS ↔ USERS ----------------
            // AssignedToUser → NO ACTION (avoid cascades)
            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.AssignedToUser)
                .WithMany() // you only have 'Tasks' (ambiguous), so omit right-side nav
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // CreatedByUser → You choose: Cascade or NoAction.
            // If you want deleting a user to delete tasks they created, use Cascade.
            // (Keep this single cascade path; we’ll make TaskTimes→Users as NoAction below to avoid multiple paths.)
            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApprovedByUser → NO ACTION (avoid cascades)
            modelBuilder.Entity<TaskModel>()
                .HasOne(t => t.ApprovedByUser)
                .WithMany()
                .HasForeignKey(t => t.ApprovedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ---------------- TASKTIMES RELATIONSHIPS ----------------
            // TaskTimes → Tasks should cascade (delete Task → delete related TaskTimes)
            modelBuilder.Entity<TaskTime>()
                .HasOne(tt => tt.Task)
                .WithMany(t => t.TaskTimes)
                .HasForeignKey(tt => tt.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskTimes → Users must NOT cascade, or you’ll get multiple cascade paths:
            // Users → Tasks (CreatedBy Cascade) → TaskTimes AND Users → TaskTimes
            modelBuilder.Entity<TaskTime>()
                .HasOne(tt => tt.User)
                .WithMany() // you don't have User.TaskTimes collection
                .HasForeignKey(tt => tt.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}