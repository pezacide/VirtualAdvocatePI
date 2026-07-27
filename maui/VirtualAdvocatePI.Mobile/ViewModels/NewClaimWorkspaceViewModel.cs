using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;
using VirtualAdvocatePI.Mobile.Navigation;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

public partial class NewClaimWorkspaceViewModel : ObservableObject
{
    private readonly IClaimWorkspaceApiClient _claimWorkspaceApiClient;
    private readonly INavigationService _navigationService;

    public NewClaimWorkspaceViewModel(
        IClaimWorkspaceApiClient claimWorkspaceApiClient,
        INavigationService navigationService)
    {
        _claimWorkspaceApiClient = claimWorkspaceApiClient;
        _navigationService = navigationService;

        WorkspaceTitle = "Post-2026 PI Claim Starter Pack";
        SelectedScenario = ScenarioOptions[^1];
    }

    public IReadOnlyList<ClaimScenarioOption> ScenarioOptions { get; } = ClaimScenarioOption.All;

    [ObservableProperty]
    public partial string WorkspaceTitle { get; set; }

    [ObservableProperty]
    public partial ClaimScenarioOption? SelectedScenario { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private async Task CreateAsync()
    {
        if (string.IsNullOrWhiteSpace(WorkspaceTitle))
        {
            ErrorMessage = "Enter a workspace title.";
            return;
        }

        if (SelectedScenario is null)
        {
            ErrorMessage = "Choose a claim preparation pathway.";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;

        try
        {
            var created = await _claimWorkspaceApiClient.CreateClaimWorkspaceAsync(
                new CreateClaimWorkspaceRequest
                {
                    WorkspaceTitle = WorkspaceTitle.Trim(),
                    ClaimScenario = SelectedScenario.Value
                });

            await _navigationService.GoToAsync(
                Routes.ClaimWorkspaceDetail,
                new Dictionary<string, object> { ["workspaceId"] = created.Id.ToString() });
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Something went wrong creating the claim workspace. Please try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private Task CancelAsync() => _navigationService.GoBackAsync();
}
