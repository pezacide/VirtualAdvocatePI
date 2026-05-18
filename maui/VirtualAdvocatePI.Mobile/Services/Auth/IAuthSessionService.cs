using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Auth;

public interface IAuthSessionService
{
    Task<AuthState> GetCurrentAuthStateAsync();

    Task<AuthState> SignInWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    Task<string?> GetIdTokenAsync();

    Task SignOutAsync();
}