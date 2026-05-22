namespace VirtualAdvocatePI.Mobile.Configuration;

public sealed class MobileAppSettings
{
    public string EnvironmentName { get; init; } = "Development";

    public string ApiBaseUrl { get; init; } = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app";

    public string ApiHealthPath { get; init; } = "/api/v1/config/secret-health";

    public string FirebaseWebApiKey { get; init; } = "AIzaSyBf5omh2wf2n_vR21B4YdFLTWcGRJRaH38";

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
            ApiBaseUrl = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app",
            ApiHealthPath = "/api/v1/config/secret-health",
            FirebaseWebApiKey = "AIzaSyBf5omh2wf2n_vR21B4YdFLTWcGRJRaH38",
            UseMockAuthentication = false,
            IsProduction = false
        };
#else
        return new MobileAppSettings
        {
            EnvironmentName = "Production",
            ApiBaseUrl = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app",
            ApiHealthPath = "/api/v1/config/secret-health",
            FirebaseWebApiKey = "AIzaSyBf5omh2wf2n_vR21B4YdFLTWcGRJRaH38",
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