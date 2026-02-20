using System.ComponentModel.DataAnnotations;
using TimeTrack.API.Models.Enums;

namespace TimeTrack.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    [Required]
    public string Department { get; set; } = string.Empty;

    public void Validate()
    {
        // Validate department
        if (!DepartmentType.IsValid(Department))
        {
            throw new ArgumentException(
                $"Invalid department. Allowed values: {DepartmentType.GetValidDepartmentsString()}");
        }

        // Validate role
        var validRoles = new[] { "Employee", "Manager", "Admin" };
        if (!validRoles.Contains(Role, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid role. Allowed values: {string.Join(", ", validRoles)}");
        }
    }
}