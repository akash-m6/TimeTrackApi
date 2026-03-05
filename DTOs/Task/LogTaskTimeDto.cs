using System.ComponentModel.DataAnnotations;

namespace TimeTrack.API.DTOs.Task;

// DTO: LogTaskTimeDto
// PURPOSE: Transfers task time logging data from frontend to backend.
public class LogTaskTimeDto
{
    [Required]
    public Guid TaskId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    [Range(0.1, 24)]
    public decimal HoursSpent { get; set; }
    
    [Required]
    [StringLength(500)]
    public string WorkDescription { get; set; }
}