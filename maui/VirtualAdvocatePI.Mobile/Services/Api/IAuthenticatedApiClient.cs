using VirtualAdvocatePI.Mobile.Models.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IAuthenticatedApiClient
{
    Task<MobileUserSession?> GetMobileSessionAsync(CancellationToken cancellationToken = default);

    Task<bool> CanReachAuthenticatedApiAsync(CancellationToken cancellationToken = default);
}