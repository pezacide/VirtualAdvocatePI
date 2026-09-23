using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class FunctionalLoadEvidenceEndpointsTests : IClassFixture<ApiTestFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ApiTestFactory _factory;

    public FunctionalLoadEvidenceEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    private sealed record IdDto(Guid Id);

    private sealed record LoadExposureRecordDto(
        Guid Id,
        Guid ClaimWorkspaceId,
        Guid? ConditionId,
        string RecordType,
        string ActivityDescription,
        string BodyAreaAffected,
        decimal? TypicalWeightKg,
        string Frequency,
        string? HazardType,
        string Status);

    private sealed record FunctionalImpactEntryDto(
        Guid Id,
        Guid ConditionId,
        string ActivityDomain,
        string? GoodDayDescription,
        string? BadDayDescription,
        string BadDayFrequency,
        string Status);

    private async Task<(HttpClient client, Guid workspaceId, Guid conditionId)> CreateWorkspaceWithConditionAsync()
    {
        var client = _factory.CreateAuthenticatedClient();

        var workspace = await client.PostAsJsonAsync("/api/v1/claim-workspaces", new { workspaceTitle = "FLE test workspace" });
        var workspaceBody = await workspace.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        var condition = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceBody!.Id}/conditions",
            new { conditionName = "Lumbar spine condition" });
        var conditionBody = await condition.Content.ReadFromJsonAsync<IdDto>(JsonOptions);

        return (client, workspaceBody.Id, conditionBody!.Id);
    }

    [Fact]
    public async Task LoadExposureRecords_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/v1/claim-workspaces/{Guid.NewGuid()}/load-exposure-records");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoadExposureRecords_ForWorkspaceOwnedByAnotherUser_Returns404()
    {
        var (_, workspaceId, _) = await CreateWorkspaceWithConditionAsync();

        var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {FakeFirebaseAuthService.SecondTestBearerToken}");

        var response = await otherClient.GetAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoadExposureRecord_RoundTripsAndAppearsInList()
    {
        var (client, workspaceId, conditionId) = await CreateWorkspaceWithConditionAsync();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records",
            new
            {
                conditionId,
                recordType = "heavy_load_carrying",
                activityDescription = "Carried filled sandbags building defensive positions",
                bodyAreaAffected = "LUMBAR_SPINE",
                typicalWeightKg = 20.5,
                frequency = "MOST_DAYS",
                yearsExposed = 6.5
            });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<LoadExposureRecordDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("HEAVY_LOAD_CARRYING", created!.RecordType);
        Assert.Equal("LUMBAR_SPINE", created.BodyAreaAffected);
        Assert.Equal(20.5m, created.TypicalWeightKg);
        Assert.Equal(conditionId, created.ConditionId);

        var list = await client.GetFromJsonAsync<List<LoadExposureRecordDto>>(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records", JsonOptions);

        Assert.NotNull(list);
        Assert.Contains(list!, r => r.Id == created.Id);
    }

    [Fact]
    public async Task CreateLoadExposureRecord_WithInvalidType_ReturnsBadRequest()
    {
        var (client, workspaceId, _) = await CreateWorkspaceWithConditionAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records",
            new { recordType = "NOT_A_REAL_TYPE", activityDescription = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateLoadExposureRecord_WithoutActivityDescription_ReturnsBadRequest()
    {
        var (client, workspaceId, _) = await CreateWorkspaceWithConditionAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records",
            new { recordType = "LIFTING_CARRYING" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PatchLoadExposureRecord_ClearsHazardTypeWhenTypeNoLongerHazard()
    {
        var (client, workspaceId, _) = await CreateWorkspaceWithConditionAsync();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records",
            new
            {
                recordType = "HAZARD_EXPOSURE",
                activityDescription = "Whole-body vibration from armoured vehicle operation",
                hazardType = "WHOLE_BODY_VIBRATION"
            });
        var created = await create.Content.ReadFromJsonAsync<LoadExposureRecordDto>(JsonOptions);
        Assert.Equal("WHOLE_BODY_VIBRATION", created!.HazardType);

        var patch = await client.PatchAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records/{created.Id}",
            new { recordType = "LIFTING_CARRYING" });

        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var patched = await patch.Content.ReadFromJsonAsync<LoadExposureRecordDto>(JsonOptions);
        Assert.Equal("LIFTING_CARRYING", patched!.RecordType);
        Assert.Null(patched.HazardType);
    }

    [Fact]
    public async Task DeleteLoadExposureRecord_ArchivesItSoItLeavesTheList()
    {
        var (client, workspaceId, _) = await CreateWorkspaceWithConditionAsync();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records",
            new { recordType = "KNEELING_SQUATTING", activityDescription = "Vehicle maintenance from kneeling" });
        var created = await create.Content.ReadFromJsonAsync<LoadExposureRecordDto>(JsonOptions);

        var delete = await client.DeleteAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        var list = await client.GetFromJsonAsync<List<LoadExposureRecordDto>>(
            $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records", JsonOptions);
        Assert.DoesNotContain(list!, r => r.Id == created.Id);
    }

    [Fact]
    public async Task FunctionalImpact_ForConditionOwnedByAnotherUser_Returns404()
    {
        var (_, workspaceId, conditionId) = await CreateWorkspaceWithConditionAsync();

        var otherClient = _factory.CreateClient();
        otherClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {FakeFirebaseAuthService.SecondTestBearerToken}");

        var response = await otherClient.GetAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateFunctionalImpactEntry_RoundTripsAndAppearsInList()
    {
        var (client, workspaceId, conditionId) = await CreateWorkspaceWithConditionAsync();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact",
            new
            {
                activityDomain = "lifting_carrying",
                goodDayDescription = "Can carry light shopping in one trip with some stiffness afterwards.",
                badDayDescription = "Cannot lift a full kettle; need help putting on shoes.",
                badDayFrequency = "WEEKLY",
                aidsOrHelpUsed = "Grabber tool; partner helps with heavier tasks."
            });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<FunctionalImpactEntryDto>(JsonOptions);
        Assert.Equal("LIFTING_CARRYING", created!.ActivityDomain);
        Assert.Equal("WEEKLY", created.BadDayFrequency);

        var list = await client.GetFromJsonAsync<List<FunctionalImpactEntryDto>>(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact", JsonOptions);
        Assert.Contains(list!, e => e.Id == created.Id);
    }

    [Fact]
    public async Task CreateFunctionalImpactEntry_WithNoGoodOrBadDay_ReturnsBadRequest()
    {
        var (client, workspaceId, conditionId) = await CreateWorkspaceWithConditionAsync();

        var response = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact",
            new { activityDomain = "SLEEP" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteFunctionalImpactEntry_ArchivesIt()
    {
        var (client, workspaceId, conditionId) = await CreateWorkspaceWithConditionAsync();

        var create = await client.PostAsJsonAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact",
            new { activityDomain = "SLEEP", badDayDescription = "Wake repeatedly from back pain." });
        var created = await create.Content.ReadFromJsonAsync<FunctionalImpactEntryDto>(JsonOptions);

        var delete = await client.DeleteAsync(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);

        var list = await client.GetFromJsonAsync<List<FunctionalImpactEntryDto>>(
            $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact", JsonOptions);
        Assert.DoesNotContain(list!, e => e.Id == created.Id);
    }

    [Fact]
    public async Task LoadReferenceItems_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/reference/load-reference-items");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task LoadReferenceItems_WhenAuthenticated_ReturnsOkWithItemsArray()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/v1/reference/load-reference-items");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("items", out var items));
        Assert.Equal(JsonValueKind.Array, items.ValueKind);
    }

    [Fact]
    public async Task LoadReferenceItemsSeed_ForNonAdmin_ReturnsForbidden()
    {
        var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsync("/api/v1/admin/reference/load-reference-items/seed", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
