using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.User;

// DTO: UserDto
// PURPOSE: Transfers user data between backend and frontend.
public class UserDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("managerId")]
    public Guid? ManagerId { get; set; }

    [JsonPropertyName("managerName")]
    public string? ManagerName { get; set; }

    [JsonPropertyName("assignedEmployeeIds")]
    public List<Guid> AssignedEmployeeIds { get; set; } = new();
}
