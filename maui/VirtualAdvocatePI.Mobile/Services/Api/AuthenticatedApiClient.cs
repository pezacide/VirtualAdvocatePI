using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.Auth;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class AuthenticatedApiClient : IAuthenticatedApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public AuthenticatedApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<MobileUserSession?> GetMobileSessionAsync(CancellationToken cancellationToken = default)
    {
        var token = await _authSessionService.GetIdTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/mobile/me");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<MobileUserSession>(
            cancellationToken: cancellationToken);
    }

    public async Task<bool> CanReachAuthenticatedApiAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await GetMobileSessionAsync(cancellationToken);

            return session?.Authenticated == true;
        }
        catch
        {
            return false;
        }
    }
}