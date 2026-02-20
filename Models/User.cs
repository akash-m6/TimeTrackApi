using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TimeTrack.API.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public Guid? ManagerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("ManagerId")]
        public User? Manager { get; set; }

        public ICollection<User> AssignedEmployees { get; set; } = new List<User>();

        public ICollection<TimeLog> TimeLogs { get; set; } = new List<TimeLog>();
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Constructor to ensure GUID is generated
        public User()
        {
            UserId = Guid.NewGuid();
        }
    }
}