using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskModel = TimeTrack.API.Models.TaskEntity;

namespace TimeTrack.API.Models;

[Table("Projects")]
public class Project
{
    [Key]
    public Guid ProjectId { get; set; } // Changed from int to Guid

    [Required]
    [StringLength(200)]
    public string ProjectName { get; set; }

    [StringLength(100)]
    public string ClientName { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Active"; // Active, Completed, OnHold, Cancelled

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Budget { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Required]
    public Guid ManagerUserId { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ManagerUserId")]
    public virtual User Manager { get; set; }

    public virtual ICollection<TaskModel> Tasks { get; set; } = new List<TaskModel>();

    // Optional: Ensure GUID is generated for new projects
    public Project()
    {
        ProjectId = Guid.NewGuid();
    }
}