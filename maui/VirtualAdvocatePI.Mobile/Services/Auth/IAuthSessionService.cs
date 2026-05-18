using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Auth;

public interface IAuthSessionService
{
    Task<AuthState> GetCurrentAuthStateAsync();

    Task<string?> GetIdTokenAsync();

    Task SignOutAsync();
}