namespace VirtualAdvocatePI.Mobile.Configuration;

public sealed class MobileAppSettings
{
    public string EnvironmentName { get; init; } = "Development";

    public string ApiBaseUrl { get; init; } = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app";

    public string ApiHealthPath { get; init; } = "/api/v1/config/secret-health";

    public bool UseMockAuthentication { get; init; } = true;

    public bool IsProduction { get; init; }

    public string DisplayName => $"{EnvironmentName} - {ApiBaseUrl}";

    public static MobileAppSettings CreateDefault()
    {
#if DEBUG
        return new MobileAppSettings
        {
            EnvironmentName = "Development",
            ApiBaseUrl = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app",
            ApiHealthPath = "/api/v1/config/secret-health",
            UseMockAuthentication = true,
            IsProduction = false
        };
#else
        return new MobileAppSettings
        {
            EnvironmentName = "Production",
            ApiBaseUrl = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app",
            ApiHealthPath = "/api/v1/config/secret-health",
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