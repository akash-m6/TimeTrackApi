using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models
{
// MODEL: Notification
// PURPOSE: Represents notification table structure in the database.
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid NotificationId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Type { get; set; } = "Info"; // Info, Warning, Success, Error

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        // Constructor to ensure GUID is generated
        public Notification()
        {
            NotificationId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}