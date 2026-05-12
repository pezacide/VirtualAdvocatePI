using System.Net;
using System.Text;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class ProtectedEndpointTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ProtectedEndpointTests(ApiTestFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/api/v1/me")]
    [InlineData("/api/v1/claim-workspaces")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/conditions")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/conditions/00000000-0000-0000-0000-000000000000/accepted-history")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/conditions/00000000-0000-0000-0000-000000000000/question-responses")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/evidence-items")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/audit-events")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/evidence-gaps")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/ai-drafts")]
    [InlineData("/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/generated-documents")]
    public async Task ProtectedGetEndpoint_WithoutBearerToken_ReturnsUnauthorized(string path)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task EvidenceUploadUrl_WithoutBearerToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var body = """
        {
          "evidenceType": "MEDICAL_REPORT",
          "originalFileName": "test.pdf",
          "fileType": "application/pdf"
        }
        """;

        var response = await client.PostAsync(
            "/api/v1/claim-workspaces/00000000-0000-0000-0000-000000000000/conditions/00000000-0000-0000-0000-000000000000/evidence-upload-url",
            new StringContent(body, Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
