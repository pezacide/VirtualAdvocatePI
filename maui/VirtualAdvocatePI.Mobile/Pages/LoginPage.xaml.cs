using VirtualAdvocatePI.Mobile.Configuration;
using VirtualAdvocatePI.Mobile.Services.Auth;

namespace VirtualAdvocatePI.Mobile.Pages;

public partial class LoginPage : ContentPage
{
    private readonly IAuthSessionService _authSessionService;
    private readonly MobileAppSettings _settings;

    public LoginPage(
        IAuthSessionService authSessionService,
        MobileAppSettings settings)
    {
        InitializeComponent();

        _authSessionService = authSessionService;
        _settings = settings;

        if (!_settings.IsFirebaseConfigured && !_settings.UseMockAuthentication)
        {
            StatusLabel.Text = "Firebase Web API key is not configured yet.";
        }
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        StatusLabel.Text = "Signing in...";

        try
        {
            var authState = await _authSessionService.SignInWithEmailPasswordAsync(
                EmailEntry.Text ?? string.Empty,
                PasswordEntry.Text ?? string.Empty);

            if (!authState.IsSignedIn)
            {
                StatusLabel.Text = "Sign-in did not complete.";
                return;
            }

            StatusLabel.Text = $"Signed in as {authState.Email}.";

            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            StatusLabel.Text = ex.Message;
        }
    }
}