using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Navigation;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

public partial class DisclaimerViewModel : ObservableObject
{
    private readonly IDisclaimerApiClient _disclaimerApiClient;
    private readonly INavigationService _navigationService;

    public DisclaimerViewModel(
        IDisclaimerApiClient disclaimerApiClient,
        INavigationService navigationService)
    {
        _disclaimerApiClient = disclaimerApiClient;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    public partial bool IsChecking { get; set; }

    [ObservableProperty]
    public partial bool IsAccepting { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private async Task CheckAcceptanceAsync()
    {
        IsChecking = true;
        ErrorMessage = null;

        try
        {
            var alreadyAccepted = await _disclaimerApiClient.GetAcceptanceStatusAsync();

            if (alreadyAccepted)
            {
                await _navigationService.GoToRootAsync(Routes.Dashboard);
            }
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Something went wrong checking the disclaimer status. Please try again.";
        }
        finally
        {
            IsChecking = false;
        }
    }

    [RelayCommand]
    private async Task AcceptAsync()
    {
        IsAccepting = true;
        ErrorMessage = null;

        try
        {
            await _disclaimerApiClient.AcceptAsync();
            await _navigationService.GoToRootAsync(Routes.Dashboard);
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Something went wrong recording your acceptance. Please try again.";
        }
        finally
        {
            IsAccepting = false;
        }
    }
}
