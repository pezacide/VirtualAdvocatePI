using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VirtualAdvocatePI.Mobile.Models.QuestionResponses;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class QuestionResponseApiClient : IQuestionResponseApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSessionService _authSessionService;

    public QuestionResponseApiClient(
        HttpClient httpClient,
        IAuthSessionService authSessionService)
    {
        _httpClient = httpClient;
        _authSessionService = authSessionService;
    }

    public async Task<IReadOnlyList<QuestionResponse>> GetQuestionResponsesAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        await EnsureSuccessAsync(response, "Could not load question responses.");

        var responses = await response.Content.ReadFromJsonAsync<List<QuestionResponse>>(
            cancellationToken: cancellationToken);

        return responses ?? new List<QuestionResponse>();
    }

    public async Task<QuestionResponse> CreateQuestionResponseAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateQuestionResponseRequest request,
        CancellationToken cancellationToken = default)
    {
        var token = await GetRequiredIdTokenAsync();

        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        await EnsureSuccessAsync(response, "Could not save question response.");

        var created = await response.Content.ReadFromJsonAsync<QuestionResponse>(
            cancellationToken: cancellationToken);

        return created ?? throw new ApiRequestException("Could not save question response.");
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
