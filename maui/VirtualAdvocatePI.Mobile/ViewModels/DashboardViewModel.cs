using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;
using VirtualAdvocatePI.Mobile.Navigation;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Dashboard;

namespace VirtualAdvocatePI.Mobile.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDashboardService _dashboardService;
    private readonly INavigationService _navigationService;

    public DashboardViewModel(
        IDashboardService dashboardService,
        INavigationService navigationService)
    {
        _dashboardService = dashboardService;
        _navigationService = navigationService;

        LoadDashboardCommand.ExecuteAsync(null);
    }

    public ObservableCollection<ClaimWorkspace> Workspaces { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool HasLoadedOnce { get; set; }

    public bool HasWorkspaces => Workspaces.Count > 0;

    public bool ShowEmptyState => HasLoadedOnce && !IsLoading && !HasError && !HasWorkspaces;

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var workspaces = await _dashboardService.GetWorkspacesAsync();

            Workspaces.Clear();

            foreach (var workspace in workspaces)
            {
                Workspaces.Add(workspace);
            }
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading your claim workspaces. Please try again.";
        }
        finally
        {
            IsLoading = false;
            HasLoadedOnce = true;

            OnPropertyChanged(nameof(HasWorkspaces));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    [RelayCommand]
    private Task CreateNewWorkspaceAsync() => _navigationService.GoToAsync(Routes.NewClaimWorkspace);

    [RelayCommand]
    private Task OpenWorkspaceAsync(ClaimWorkspace? workspace)
    {
        if (workspace is null)
        {
            return Task.CompletedTask;
        }

        return _navigationService.GoToAsync(
            Routes.ClaimWorkspaceDetail,
            new Dictionary<string, object> { ["workspaceId"] = workspace.Id.ToString() });
    }
}
