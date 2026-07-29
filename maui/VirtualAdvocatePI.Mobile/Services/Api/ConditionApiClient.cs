using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class ConditionApiClient : IConditionApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public ConditionApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<ClaimCondition>> GetConditionsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load conditions.");

        var conditions = await response.Content.ReadFromJsonAsync<List<ClaimCondition>>(
            cancellationToken: cancellationToken);

        return conditions ?? new List<ClaimCondition>();
    }

    public async Task<ClaimCondition> CreateConditionAsync(
        Guid workspaceId,
        CreateConditionRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not create condition.");

        var condition = await response.Content.ReadFromJsonAsync<ClaimCondition>(
            cancellationToken: cancellationToken);

        return condition ?? throw new ApiRequestException("Could not create condition.");
    }

    public async Task ArchiveConditionAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not remove condition.");
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

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new ApiRequestException($"{defaultMessage} {body}");
        }

        throw new ApiRequestException($"{defaultMessage} (HTTP {(int)response.StatusCode})");
    }
}
