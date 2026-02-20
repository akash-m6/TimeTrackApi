using System.ComponentModel.DataAnnotations;

namespace TimeTrack.API.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Type { get; set; }

    [Required]
    [StringLength(500)]
    public string Message { get; set; }
}