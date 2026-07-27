using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class ClaimWorkspaceEndpointsTests : IClassFixture<ApiTestFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiTestFactory _factory;

    public ClaimWorkspaceEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    private sealed record ClaimWorkspaceDto(Guid Id, string ClaimScenario, string WorkspaceTitle, string Status);

    [Fact]
    public async Task CreateWorkspace_WithoutInput_DefaultsScenarioAndTitle()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/v1/claim-workspaces", new { });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        Assert.NotNull(body);
        Assert.Equal("UNSURE", body!.ClaimScenario);
        Assert.Equal("Post-2026 PI Claim Starter Pack", body.WorkspaceTitle);
    }

    [Fact]
    public async Task CreateWorkspace_WithInvalidScenario_ReturnsBadRequest()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/claim-workspaces",
            new { claimScenario = "NOT_A_REAL_SCENARIO" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetWorkspaces_OnlyReturnsCallersOwnNonArchivedWorkspaces()
    {
        var client = _factory.CreateAuthenticatedClient();

        var created = await client.PostAsJsonAsync("/api/v1/claim-workspaces", new { workspaceTitle = "List test workspace" });
        var createdBody = await created.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        var listResponse = await client.GetAsync("/api/v1/claim-workspaces");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var list = await listResponse.Content.ReadFromJsonAsync<List<ClaimWorkspaceDto>>(JsonOptions);

        Assert.NotNull(list);
        Assert.Contains(list!, w => w.Id == createdBody!.Id);
    }

    [Fact]
    public async Task PatchWorkspace_WithInvalidStatus_ReturnsBadRequestAndLeavesWorkspaceUnchanged()
    {
        var client = _factory.CreateAuthenticatedClient();

        var created = await client.PostAsJsonAsync("/api/v1/claim-workspaces", new { });
        var createdBody = await created.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        var patchResponse = await client.PatchAsJsonAsync(
            $"/api/v1/claim-workspaces/{createdBody!.Id}",
            new { status = "NOT_A_REAL_STATUS" });

        Assert.Equal(HttpStatusCode.BadRequest, patchResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/claim-workspaces/{createdBody.Id}");
        var getBody = await getResponse.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        Assert.Equal("IN_PROGRESS", getBody!.Status);
    }

    [Fact]
    public async Task DeleteWorkspace_ArchivesItSoItNoLongerAppearsOrIsFetchable()
    {
        var client = _factory.CreateAuthenticatedClient();

        var created = await client.PostAsJsonAsync("/api/v1/claim-workspaces", new { });
        var createdBody = await created.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        var deleteResponse = await client.DeleteAsync($"/api/v1/claim-workspaces/{createdBody!.Id}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/v1/claim-workspaces/{createdBody.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetWorkspace_BelongingToAnotherUser_ReturnsNotFound()
    {
        var ownerClient = _factory.CreateAuthenticatedClient();
        var created = await ownerClient.PostAsJsonAsync("/api/v1/claim-workspaces", new { });
        var createdBody = await created.Content.ReadFromJsonAsync<ClaimWorkspaceDto>(JsonOptions);

        var otherUserClient = _factory.CreateClient();
        otherUserClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {FakeFirebaseAuthService.SecondTestBearerToken}");

        var response = await otherUserClient.GetAsync($"/api/v1/claim-workspaces/{createdBody!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
