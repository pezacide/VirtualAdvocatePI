namespace VirtualAdvocatePI.Mobile.Configuration;

public sealed class MobileAppSettings
{
    public string ApiBaseUrl { get; init; } = "https://vapi-dev-api-2pwcdyx42q-ts.a.run.app";

    public string EnvironmentName { get; init; } = "Development";

    public bool UseMockAuthentication { get; init; } = true;
}