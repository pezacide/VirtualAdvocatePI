using VirtualAdvocatePI.Mobile.Configuration;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class VirtualAdvocateApiClient : IVirtualAdvocateApiClient
{
    private readonly HttpClient _httpClient;
    private readonly MobileAppSettings _settings;

    public VirtualAdvocateApiClient(
        HttpClient httpClient,
        MobileAppSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public Task<string> GetApiBaseUrlAsync()
    {
        return Task.FromResult(_settings.ApiBaseUrl);
    }

    public async Task<bool> CanReachApiAsync(CancellationToken cancellationToken = default)
    {
        if (!_settings.IsValid())
        {
            return false;
        }

        try
        {
            using var response = await _httpClient.GetAsync(_settings.ApiHealthPath, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}