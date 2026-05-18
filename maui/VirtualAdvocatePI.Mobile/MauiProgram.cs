using Microsoft.Extensions.Logging;
using VirtualAdvocatePI.Mobile.Configuration;
using VirtualAdvocatePI.Mobile.Pages;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Auth;

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

        builder.Services.AddSingleton<IAuthSessionService, MockAuthSessionService>();
        builder.Services.AddTransient<HomePage>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(settings.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        });

        builder.Services.AddSingleton<IVirtualAdvocateApiClient, VirtualAdvocateApiClient>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}