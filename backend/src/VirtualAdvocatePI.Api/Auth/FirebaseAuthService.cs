using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace VirtualAdvocatePI.Api.Auth;

public sealed class FirebaseAuthService : IFirebaseAuthService
{
    private readonly string _projectId;

    public FirebaseAuthService(IConfiguration configuration)
    {
        _projectId =
            configuration["Firebase:ProjectId"]
            ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID")
            ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_PROJECT")
            ?? "dva-sop-dev";
    }

    public async Task<AuthenticatedFirebaseUser?> VerifyBearerTokenAsync(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return null;
        }

        var authorizationValue = authorizationHeader.ToString();

        if (!authorizationValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var idToken = authorizationValue["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        EnsureFirebaseApp();

        var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);

        decodedToken.Claims.TryGetValue("email", out var emailClaim);
        decodedToken.Claims.TryGetValue("name", out var nameClaim);

        return new AuthenticatedFirebaseUser(
            FirebaseUid: decodedToken.Uid,
            Email: emailClaim?.ToString(),
            DisplayName: nameClaim?.ToString()
        );
    }

    private void EnsureFirebaseApp()
    {
        if (FirebaseApp.DefaultInstance is not null)
        {
            return;
        }

        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.GetApplicationDefault(),
            ProjectId = _projectId
        });
    }
}
