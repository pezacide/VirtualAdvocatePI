using VirtualAdvocatePI.Mobile.Configuration;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public sealed class MobileEnvironmentService : IMobileEnvironmentService
{
    public MobileEnvironmentService(MobileAppSettings settings)
    {
        Current = settings;
    }

    public MobileAppSettings Current { get; }

    public string GetEnvironmentSummary()
    {
        var authMode = Current.UseMockAuthentication
            ? "Mock authentication"
            : "Firebase authentication";

        return $"Environment: {Current.EnvironmentName}\nAPI: {Current.ApiBaseUrl}\nAuth mode: {authMode}";
    }

    public bool IsApiConfigurationValid()
    {
        return Current.IsValid();
    }
}