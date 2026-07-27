using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class ClaimWorkspaceApiClient : IClaimWorkspaceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public ClaimWorkspaceApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<ClaimWorkspace>> GetClaimWorkspacesAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/claim-workspaces");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load claim workspaces.");

        var workspaces = await response.Content.ReadFromJsonAsync<List<ClaimWorkspace>>(
            cancellationToken: cancellationToken);

        return workspaces ?? new List<ClaimWorkspace>();
    }

    public async Task<ClaimWorkspace> GetClaimWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/claim-workspaces/{workspaceId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load claim workspace.");

        var workspace = await response.Content.ReadFromJsonAsync<ClaimWorkspace>(cancellationToken: cancellationToken);

        return workspace ?? throw new ApiRequestException("Could not load claim workspace.");
    }

    public async Task<ClaimWorkspace> CreateClaimWorkspaceAsync(
        CreateClaimWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/claim-workspaces")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not create claim workspace.");

        var workspace = await response.Content.ReadFromJsonAsync<ClaimWorkspace>(cancellationToken: cancellationToken);

        return workspace ?? throw new ApiRequestException("Could not create claim workspace.");
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

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ApiRequestException("The requested resource was not found.");
        }

        throw new ApiRequestException($"{defaultMessage} (HTTP {(int)response.StatusCode})");
    }
}
