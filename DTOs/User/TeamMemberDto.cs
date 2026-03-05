using System.Text.Json.Serialization;

namespace TimeTrack.API.DTOs.User;

// DTO: TeamMemberDto
// PURPOSE: Transfers team member data for manager dashboard between backend and frontend.
public class TeamMemberDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}
