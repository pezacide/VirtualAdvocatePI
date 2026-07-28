namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IDisclaimerApiClient
{
    Task<bool> GetAcceptanceStatusAsync(CancellationToken cancellationToken = default);

    Task AcceptAsync(CancellationToken cancellationToken = default);
}
