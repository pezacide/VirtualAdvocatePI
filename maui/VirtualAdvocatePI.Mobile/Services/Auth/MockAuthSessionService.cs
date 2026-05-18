using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Auth;

public sealed class MockAuthSessionService : IAuthSessionService
{
    public Task<AuthState> GetCurrentAuthStateAsync()
    {
        return Task.FromResult(new AuthState
        {
            IsSignedIn = false
        });
    }

    public Task<AuthState> SignInWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AuthState
        {
            IsSignedIn = true,
            Email = email,
            DisplayName = email,
            IdToken = "mock-token",
            RefreshToken = "mock-refresh-token",
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddHours(1)
        });
    }

    public Task<string?> GetIdTokenAsync()
    {
        return Task.FromResult<string?>("mock-token");
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }
}