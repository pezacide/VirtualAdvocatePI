using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class HomePage : ContentPage
{
    private readonly IVirtualAdvocateApiClient _apiClient;
    private readonly IMobileEnvironmentService _environmentService;

    public HomePage(
        IVirtualAdvocateApiClient apiClient,
        IMobileEnvironmentService environmentService)
    {
        InitializeComponent();

        _apiClient = apiClient;
        _environmentService = environmentService;

        EnvironmentLabel.Text = _environmentService.GetEnvironmentSummary();
    }

    private async void OnCheckApiConnectionClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Checking API connection...";

        if (!_environmentService.IsApiConfigurationValid())
        {
            StatusLabel.Text = "API configuration is invalid. Check the mobile environment settings.";
            return;
        }

        var apiBaseUrl = await _apiClient.GetApiBaseUrlAsync();
        var canReachApi = await _apiClient.CanReachApiAsync();

        StatusLabel.Text = canReachApi
            ? $"API reachable: {apiBaseUrl}"
            : $"API not reachable yet: {apiBaseUrl}";
    }
}