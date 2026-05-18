using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class HomePage : ContentPage
{
    private readonly IVirtualAdvocateApiClient _apiClient;
    private readonly IMobileEnvironmentService _environmentService;
    private readonly IAuthSessionService _authSessionService;

    public HomePage(
        IVirtualAdvocateApiClient apiClient,
        IMobileEnvironmentService environmentService,
        IAuthSessionService authSessionService)
    {
        InitializeComponent();

        _apiClient = apiClient;
        _environmentService = environmentService;
        _authSessionService = authSessionService;

        EnvironmentLabel.Text = _environmentService.GetEnvironmentSummary();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var authState = await _authSessionService.GetCurrentAuthStateAsync();

        AuthStatusLabel.Text = authState.IsSignedIn
            ? $"Signed in as {authState.Email}"
            : "Not signed in.";
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

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        await _authSessionService.SignOutAsync();

        AuthStatusLabel.Text = "Signed out.";

        await Shell.Current.GoToAsync("//LoginPage");
    }
}