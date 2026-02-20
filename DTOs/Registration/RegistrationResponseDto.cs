namespace TimeTrack.API.DTOs.Registration
{
    public class RegistrationResponseDto
    {
        public bool Success { get; set; }

        public Guid RegistrationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Optional, but you are populating these in the service—expose them:
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
