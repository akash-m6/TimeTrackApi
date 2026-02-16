using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models;

[Table("Tasks")]
public class TaskEntity
{
    [Key]
    public int TaskId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    public int AssignedToUserId { get; set; }

    [Required]
    public int CreatedByUserId { get; set; }

    public int? ProjectId { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal EstimatedHours { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Approved

    [StringLength(50)]
    public string Priority { get; set; } = "Medium";

    public DateTime? DueDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? StartedDate { get; set; }  // NEW: Track when task was started

    public DateTime? CompletedDate { get; set; }

    public bool IsApproved { get; set; } = false;  // NEW: Manager approval flag

    public DateTime? ApprovedDate { get; set; }  // NEW: When manager approved

    public int? ApprovedByUserId { get; set; }  // NEW: Who approved

    // Navigation Properties
    [ForeignKey("AssignedToUserId")]
    public virtual UserEntity AssignedToUser { get; set; }

    [ForeignKey("CreatedByUserId")]
    public virtual UserEntity CreatedByUser { get; set; }

    [ForeignKey("ProjectId")]
    public virtual ProjectEntity Project { get; set; }

    [ForeignKey("ApprovedByUserId")]
    public virtual UserEntity ApprovedByUser { get; set; }

    public virtual ICollection<TaskTimeEntity> TaskTimes { get; set; } = new List<TaskTimeEntity>();
}