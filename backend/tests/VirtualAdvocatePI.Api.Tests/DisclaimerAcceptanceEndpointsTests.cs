using System.Net;
using System.Net.Http.Json;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class DisclaimerAcceptanceEndpointsTests : IClassFixture<ApiTestFactory>
{
    private sealed record AcceptanceStatus(bool Accepted);

    private readonly ApiTestFactory _factory;

    public DisclaimerAcceptanceEndpointsTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAcceptance_BeforeAccepting_ReturnsFalse()
    {
        // Uses the second fake user so this doesn't depend on running before
        // PostAcceptance_ThenGet_ReturnsTrue within the shared test factory.
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {FakeFirebaseAuthService.SecondTestBearerToken}");

        var response = await client.GetAsync("/api/v1/me/disclaimer-acceptance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AcceptanceStatus>();

        Assert.False(body!.Accepted);
    }

    [Fact]
    public async Task PostAcceptance_ThenGet_ReturnsTrue()
    {
        var client = _factory.CreateAuthenticatedClient();

        var postResponse = await client.PostAsync("/api/v1/me/disclaimer-acceptance", content: null);
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

        var getResponse = await client.GetAsync("/api/v1/me/disclaimer-acceptance");
        var body = await getResponse.Content.ReadFromJsonAsync<AcceptanceStatus>();

        Assert.True(body!.Accepted);
    }

    [Fact]
    public async Task GetAcceptance_WithoutBearerToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/me/disclaimer-acceptance");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
