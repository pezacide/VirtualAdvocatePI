namespace VirtualAdvocatePI.Mobile.Configuration;

public sealed class MobileAppSettings
{
    // There is currently only one deployed backend (the dva-sop-dev Cloud Run service), so
    // debug and release builds intentionally point at the same API and Firebase project until
    // a real production backend exists.
    private const string DevApiBaseUrl = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app";
    private const string DevApiHealthPath = "/api/v1/config/secret-health";
    private const string DevFirebaseWebApiKey = "AIzaSyBf5omh2wf2n_vR21B4YdFLTWcGRJRaH38";

    public string EnvironmentName { get; init; } = "Development";

    public string ApiBaseUrl { get; init; } = DevApiBaseUrl;

    public string ApiHealthPath { get; init; } = DevApiHealthPath;

    public string FirebaseWebApiKey { get; init; } = DevFirebaseWebApiKey;

    public bool UseMockAuthentication { get; init; } = false;

    public bool IsProduction { get; init; }

    public bool IsFirebaseConfigured =>
        !string.IsNullOrWhiteSpace(FirebaseWebApiKey) &&
        !FirebaseWebApiKey.Contains("PASTE_FIREBASE_WEB_API_KEY_HERE", StringComparison.OrdinalIgnoreCase) &&
        !FirebaseWebApiKey.Contains("YOUR_FIREBASE", StringComparison.OrdinalIgnoreCase);

    public string DisplayName => $"{EnvironmentName} - {ApiBaseUrl}";

    public static MobileAppSettings CreateDefault()
    {
#if DEBUG
        return new MobileAppSettings
        {
            EnvironmentName = "Development",
            ApiBaseUrl = DevApiBaseUrl,
            ApiHealthPath = DevApiHealthPath,
            FirebaseWebApiKey = DevFirebaseWebApiKey,
            UseMockAuthentication = false,
            IsProduction = false
        };
#else
        return new MobileAppSettings
        {
            EnvironmentName = "Production",
            ApiBaseUrl = DevApiBaseUrl,
            ApiHealthPath = DevApiHealthPath,
            FirebaseWebApiKey = DevFirebaseWebApiKey,
            UseMockAuthentication = false,
            IsProduction = true
        };
#endif
    }

    public bool IsValid()
    {
        return Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttps || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase));
    }
}