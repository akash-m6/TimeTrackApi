using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models
{
    public class TaskTime
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid TaskTimeId { get; set; }

        [Required]
        public Guid TaskId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal HoursSpent { get; set; }

        [MaxLength(500)]
        public string? WorkDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(TaskId))]
        public TaskEntity Task { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Constructor to ensure GUID is generated
        public TaskTime()
        {
            TaskTimeId = Guid.NewGuid();
        }
    }
}
