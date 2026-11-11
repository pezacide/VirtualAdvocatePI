using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.AiDrafts;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class AiDraftApiClient : IAiDraftApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public AiDraftApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<AiDraft>> GetConditionAiDraftsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/ai-drafts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load AI drafts.");

        var drafts = await response.Content.ReadFromJsonAsync<List<AiDraft>>(
            cancellationToken: cancellationToken);

        return drafts ?? new List<AiDraft>();
    }

    public async Task<AiDraft> UpdateAiDraftAsync(
        Guid workspaceId,
        Guid draftId,
        UpdateAiDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Patch,
            $"/api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not update this AI draft.");

        var draft = await response.Content.ReadFromJsonAsync<AiDraft>(
            cancellationToken: cancellationToken);

        return draft ?? throw new ApiRequestException("Could not update this AI draft.");
    }

    public async Task ArchiveAiDraftAsync(
        Guid workspaceId,
        Guid draftId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draftId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not archive this AI draft.");
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
