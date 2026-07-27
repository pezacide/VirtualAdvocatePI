using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class ClaimWorkspaceDetailViewModel : ObservableObject
{
    private readonly IClaimWorkspaceApiClient _claimWorkspaceApiClient;

    public ClaimWorkspaceDetailViewModel(IClaimWorkspaceApiClient claimWorkspaceApiClient)
    {
        _claimWorkspaceApiClient = claimWorkspaceApiClient;

        WorkspaceId = string.Empty;
    }

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

    [ObservableProperty]
    public partial ClaimWorkspace? Workspace { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadWorkspaceCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadWorkspaceAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var id))
        {
            return;
        }

        IsLoading = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            Workspace = await _claimWorkspaceApiClient.GetClaimWorkspaceAsync(id);
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading this claim workspace. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
