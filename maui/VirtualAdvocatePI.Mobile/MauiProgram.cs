using Microsoft.Extensions.Logging;
using VirtualAdvocatePI.Mobile.Configuration;
using VirtualAdvocatePI.Mobile.Navigation;
using VirtualAdvocatePI.Mobile.Pages;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Auth;
using VirtualAdvocatePI.Mobile.Services.Dashboard;
using VirtualAdvocatePI.Mobile.ViewModels;

namespace VirtualAdvocatePI.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var settings = MobileAppSettings.CreateDefault();

        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton<IMobileEnvironmentService, MobileEnvironmentService>();

        if (settings.UseMockAuthentication)
        {
            builder.Services.AddSingleton<IAuthSessionService, MockAuthSessionService>();
        }
        else
        {
            builder.Services.AddSingleton<IAuthSessionService, FirebaseAuthSessionService>();
        }
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<NewClaimWorkspacePage>();
        builder.Services.AddTransient<NewClaimWorkspaceViewModel>();
        builder.Services.AddTransient<ClaimWorkspaceDetailPage>();
        builder.Services.AddTransient<ClaimWorkspaceDetailViewModel>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        builder.Services.AddSingleton<IVirtualAdvocateApiClient, VirtualAdvocateApiClient>();
        builder.Services.AddSingleton<IAuthenticatedApiClient, AuthenticatedApiClient>();
        builder.Services.AddSingleton<IClaimWorkspaceApiClient, ClaimWorkspaceApiClient>();
        builder.Services.AddSingleton<IDashboardService, DashboardService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}