using Microsoft.AspNetCore.Http;
using VirtualAdvocatePI.Api.Auth;

namespace VirtualAdvocatePI.Api.Tests;

public sealed class FakeFirebaseAuthService : IFirebaseAuthService
{
    public const string TestBearerToken = "test-token";
    public const string SecondTestBearerToken = "test-token-2";

    public static readonly AuthenticatedFirebaseUser TestUser = new(
        FirebaseUid: "test-firebase-uid",
        Email: "veteran@example.test",
        DisplayName: "Test Veteran");

    public static readonly AuthenticatedFirebaseUser SecondTestUser = new(
        FirebaseUid: "test-firebase-uid-2",
        Email: "other-veteran@example.test",
        DisplayName: "Other Test Veteran");

    public Task<AuthenticatedFirebaseUser?> VerifyBearerTokenAsync(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Task.FromResult<AuthenticatedFirebaseUser?>(null);
        }

        var authorizationValue = authorizationHeader.ToString();

        if (!authorizationValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<AuthenticatedFirebaseUser?>(null);
        }

        var token = authorizationValue["Bearer ".Length..].Trim();

        AuthenticatedFirebaseUser? user = token switch
        {
            TestBearerToken => TestUser,
            SecondTestBearerToken => SecondTestUser,
            _ => null
        };

        return Task.FromResult(user);
    }
}
