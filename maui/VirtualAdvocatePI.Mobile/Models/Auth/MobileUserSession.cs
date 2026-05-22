namespace VirtualAdvocatePI.Mobile.Models.Auth;

public sealed class MobileUserSession
{
    public Guid UserId { get; set; }

    public string FirebaseUid { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string Role { get; set; } = string.Empty;

    public string AccountStatus { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    public bool Authenticated { get; set; }
}