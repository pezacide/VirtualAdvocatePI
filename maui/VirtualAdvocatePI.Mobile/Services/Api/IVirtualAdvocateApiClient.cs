namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IVirtualAdvocateApiClient
{
    Task<string> GetApiBaseUrlAsync();

    Task<bool> CanReachApiAsync(CancellationToken cancellationToken = default);
}