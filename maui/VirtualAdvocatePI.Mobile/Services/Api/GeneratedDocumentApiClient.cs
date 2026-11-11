using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class GeneratedDocumentApiClient : IGeneratedDocumentApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public GeneratedDocumentApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<GeneratedDocument>> GetGeneratedDocumentsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/generated-documents");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load generated documents.");

        var documents = await response.Content.ReadFromJsonAsync<List<GeneratedDocument>>(
            cancellationToken: cancellationToken);

        return documents ?? new List<GeneratedDocument>();
    }

    public Task<GeneratePackResponse> GenerateClaimStarterPackAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default) =>
        GeneratePackAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/generated-documents/claim-starter-pack",
            "Could not generate the Claim Starter Pack.",
            cancellationToken);

    public Task<GeneratePackResponse> GenerateDoctorGuidancePackAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default) =>
        GeneratePackAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/generated-documents/doctor-guidance-pack",
            "Could not generate the Doctor Guidance Pack.",
            cancellationToken);

    private async Task<GeneratePackResponse> GeneratePackAsync(
        string route,
        string defaultMessage,
        CancellationToken cancellationToken)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = JsonContent.Create(new { })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, defaultMessage);

        var result = await response.Content.ReadFromJsonAsync<GeneratePackResponse>(
            cancellationToken: cancellationToken);

        return result ?? throw new ApiRequestException(defaultMessage);
    }

    public async Task<GeneratedDocumentDownloadUrlResponse> CreateDownloadUrlAsync(
        Guid workspaceId,
        Guid documentId,
        string format,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/generated-documents/{documentId}/download-url")
        {
            Content = JsonContent.Create(new { format })
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, $"Could not create a {format} download link.");

        var result = await response.Content.ReadFromJsonAsync<GeneratedDocumentDownloadUrlResponse>(
            cancellationToken: cancellationToken);

        if (result is null || string.IsNullOrWhiteSpace(result.Url))
        {
            throw new ApiRequestException($"Could not create a {format} download link.");
        }

        return result;
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
