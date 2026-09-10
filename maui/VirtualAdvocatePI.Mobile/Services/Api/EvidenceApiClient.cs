using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.Evidence;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class EvidenceApiClient : IEvidenceApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    // Direct-to-storage transfers must NOT carry the API base address or the
    // Firebase bearer token, so they use a dedicated long-timeout client.
    private static readonly HttpClient TransferClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public EvidenceApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<EvidenceItem>> GetConditionEvidenceItemsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load evidence items.");

        var items = await response.Content.ReadFromJsonAsync<List<EvidenceItem>>(
            cancellationToken: cancellationToken);

        return items ?? new List<EvidenceItem>();
    }

    public async Task<EvidenceItem> CreateEvidenceItemAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateEvidenceItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-items")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not add evidence item.");

        var item = await response.Content.ReadFromJsonAsync<EvidenceItem>(
            cancellationToken: cancellationToken);

        return item ?? throw new ApiRequestException("Could not add evidence item.");
    }

    public async Task<EvidenceUploadUrlResponse> CreateEvidenceUploadUrlAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateEvidenceUploadUrlRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-upload-url")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not start the file upload.");

        var result = await response.Content.ReadFromJsonAsync<EvidenceUploadUrlResponse>(
            cancellationToken: cancellationToken);

        if (result?.Upload is null || string.IsNullOrWhiteSpace(result.Upload.Url) || result.EvidenceItem is null)
        {
            throw new ApiRequestException("Could not start the file upload.");
        }

        return result;
    }

    public async Task UploadFileToSignedUrlAsync(
        string signedUrl,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);

        using var response = await TransferClient.PutAsync(signedUrl, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new ApiRequestException(
                $"The file could not be uploaded to secure storage (HTTP {(int)response.StatusCode}).");
        }
    }

    public async Task<EvidenceItem> MarkEvidenceUploadedAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/mark-uploaded");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "The upload finished but could not be confirmed.");

        var item = await response.Content.ReadFromJsonAsync<EvidenceItem>(
            cancellationToken: cancellationToken);

        return item ?? throw new ApiRequestException("The upload finished but could not be confirmed.");
    }

    public async Task<EvidenceDownloadUrlResponse> CreateEvidenceDownloadUrlAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}/download-url");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not create a download link for this file.");

        var result = await response.Content.ReadFromJsonAsync<EvidenceDownloadUrlResponse>(
            cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ApiRequestException("Could not create a download link for this file.");
        }

        return result;
    }

    public async Task ArchiveEvidenceItemAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItemId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not remove this evidence item.");
    }

    public async Task<IReadOnlyList<EvidenceGap>> GetConditionEvidenceGapsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load evidence gaps.");

        var gaps = await response.Content.ReadFromJsonAsync<List<EvidenceGap>>(
            cancellationToken: cancellationToken);

        return gaps ?? new List<EvidenceGap>();
    }

    public async Task<IReadOnlyList<EvidenceGap>> RecalculateEvidenceGapsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/evidence-gaps/recalculate");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not recalculate evidence gaps.");

        // The recalculate endpoint returns only the newly created gaps, so read
        // back the full current list for display.
        return await GetConditionEvidenceGapsAsync(workspaceId, conditionId, cancellationToken);
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
