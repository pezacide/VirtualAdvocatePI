using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Auth;

public sealed class MockAuthSessionService : IAuthSessionService
{
    public Task<AuthState> GetCurrentAuthStateAsync()
    {
        return Task.FromResult(new AuthState
        {
            IsSignedIn = false,
            DisplayName = null,
            Email = null,
            IdToken = null
        });
    }

    public Task<string?> GetIdTokenAsync()
    {
        return Task.FromResult<string?>(null);
    }

    public Task SignOutAsync()
    {
        return Task.CompletedTask;
    }
}