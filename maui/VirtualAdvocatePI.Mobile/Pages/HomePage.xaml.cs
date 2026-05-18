using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class HomePage : ContentPage
{
    private readonly IVirtualAdvocateApiClient _apiClient;

    public HomePage(IVirtualAdvocateApiClient apiClient)
    {
        InitializeComponent();

        _apiClient = apiClient;
    }

    private async void OnCheckApiConnectionClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Checking API connection...";

        var apiBaseUrl = await _apiClient.GetApiBaseUrlAsync();
        var canReachApi = await _apiClient.CanReachApiAsync();

        StatusLabel.Text = canReachApi
            ? $"API reachable: {apiBaseUrl}"
            : $"API not reachable yet: {apiBaseUrl}";
    }
}