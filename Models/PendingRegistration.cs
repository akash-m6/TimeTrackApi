using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TimeTrack.API.Models;

namespace TimeTrack.API.Models;

[Table("PendingRegistrations")]
public class PendingRegistration
{
    [Key]
    public Guid RegistrationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty; // Employee, Manager

    [Required]
    [StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedDate { get; set; }

    public Guid? ProcessedByUserId { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    // Navigation Property for the admin who processed
    [ForeignKey("ProcessedByUserId")]
    public virtual User? ProcessedByUser { get; set; }
}