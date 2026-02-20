using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models
{
    public class TaskEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaskId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        public Guid AssignedToUserId { get; set; }

        [Required]
        public Guid CreatedByUserId { get; set; }

        public Guid? ProjectId { get; set; }

        public decimal EstimatedHours { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Approved

        [MaxLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public DateTime? DueDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public Guid? ApprovedByUserId { get; set; }

        // Navigation properties
        [ForeignKey(nameof(AssignedToUserId))]
        public User AssignedToUser { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUserId))]
        public User CreatedByUser { get; set; } = null!;

        [ForeignKey(nameof(ProjectId))]
        public Project? Project { get; set; }

        [ForeignKey(nameof(ApprovedByUserId))]
        public User ApprovedByUser { get; set; }

        public ICollection<TaskTime> TaskTimes { get; set; } = new List<TaskTime>();

        // Constructor to ensure GUID is generated
        public TaskEntity()
        {
            TaskId = Guid.NewGuid();
        }
    }
}