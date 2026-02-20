namespace TimeTrack.API.DTOs.User;

public class TeamMemberDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
