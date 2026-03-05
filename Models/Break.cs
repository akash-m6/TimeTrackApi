using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models;


// MODEL: Break
// PURPOSE: Represents break table structure in the database.
[Table("Breaks")]
public class Break
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid BreakId { get; set; }

    [Required]
    public Guid TimeLogId { get; set; }

    [Required]
    [StringLength(100)]
    public string Activity { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "time")]
    public TimeSpan StartTime { get; set; }

    [Column(TypeName = "time")]
    public TimeSpan? EndTime { get; set; }

    public int? Duration { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(TimeLogId))]
    public TimeLog TimeLog { get; set; } = null!;

    public Break()
    {
        BreakId = Guid.NewGuid();
    }
}
