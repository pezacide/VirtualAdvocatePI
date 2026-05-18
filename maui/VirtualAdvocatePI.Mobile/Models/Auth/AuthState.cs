namespace VirtualAdvocatePI.Mobile.Models.Auth;

public sealed class AuthState
{
    public bool IsSignedIn { get; init; }

    public string? DisplayName { get; init; }

    public string? Email { get; init; }

    public string? IdToken { get; init; }
}