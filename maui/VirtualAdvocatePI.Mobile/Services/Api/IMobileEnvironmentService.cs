using VirtualAdvocatePI.Mobile.Configuration;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IMobileEnvironmentService
{
    MobileAppSettings Current { get; }

    string GetEnvironmentSummary();

    bool IsApiConfigurationValid();
}