using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.Disclaimer;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class DisclaimerApiClient : IDisclaimerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public DisclaimerApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<bool> GetAcceptanceStatusAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/me/disclaimer-acceptance");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not check disclaimer acceptance status.");

        var status = await response.Content.ReadFromJsonAsync<DisclaimerAcceptanceStatus>(
            cancellationToken: cancellationToken);

        return status?.Accepted ?? false;
    }

    public async Task AcceptAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/me/disclaimer-acceptance");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not record disclaimer acceptance.");
    }

    private async Task<string> GetRequiredIdTokenAsync()
    {
        var token = await _authSessionService.GetIdTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ApiRequestException("You are not signed in. Please sign in again.");
        }

        return token;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string defaultMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new ApiRequestException("Your session has expired. Please sign in again.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new ApiRequestException("You do not have permission to access this resource.");
        }

        throw new ApiRequestException($"{defaultMessage} (HTTP {(int)response.StatusCode})");
    }
}
