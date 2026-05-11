namespace VirtualAdvocatePI.Api.Domain.Users;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirebaseUid { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string Role { get; set; } = "VETERAN";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    public string AccountStatus { get; set; } = "ACTIVE";
}
