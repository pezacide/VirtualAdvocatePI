using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.AiDrafts;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Dialogs;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class AiDraftReviewViewModel : ObservableObject
{
    private readonly IConditionApiClient _conditionApiClient;
    private readonly IAiDraftApiClient _aiDraftApiClient;
    private readonly IDialogService _dialogService;

    public AiDraftReviewViewModel(
        IConditionApiClient conditionApiClient,
        IAiDraftApiClient aiDraftApiClient,
        IDialogService dialogService)
    {
        _conditionApiClient = conditionApiClient;
        _aiDraftApiClient = aiDraftApiClient;
        _dialogService = dialogService;

        WorkspaceId = string.Empty;
        EditedText = string.Empty;
    }

    public const string SafetyBoundary =
        "Preparation support only. AI-assisted text must be reviewed by you before it is used anywhere. " +
        "It is not legal advice, not medical advice, not a diagnosis, not a DVA decision, not a GARP M " +
        "impairment calculation, not a compensation estimate, and not a guarantee of a claim outcome.";

    public ObservableCollection<ClaimCondition> Conditions { get; } = new();

    public ObservableCollection<AiDraft> Drafts { get; } = new();

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool IsLoadingConditions { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool HasLoadedConditionsOnce { get; set; }

    [ObservableProperty]
    public partial ClaimCondition? SelectedCondition { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyDraftsState))]
    public partial bool IsLoadingDrafts { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyDraftsState))]
    public partial bool HasLoadedDraftsOnce { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedDraft))]
    public partial AiDraft? SelectedDraft { get; set; }

    [ObservableProperty]
    public partial string EditedText { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    public bool HasConditions => Conditions.Count > 0;

    public bool ShowNoConditionsState => HasLoadedConditionsOnce && !IsLoadingConditions && !HasConditions;

    public bool HasDrafts => Drafts.Count > 0;

    public bool ShowEmptyDraftsState =>
        HasLoadedDraftsOnce && !IsLoadingDrafts && SelectedCondition is not null && !HasDrafts;

    public bool HasSelectedDraft => SelectedDraft is not null;

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadConditionsCommand.ExecuteAsync(null);
    }

    partial void OnSelectedConditionChanged(ClaimCondition? value)
    {
        SelectedDraft = null;

        if (value is not null)
        {
            LoadDraftsCommand.ExecuteAsync(null);
        }
    }

    partial void OnSelectedDraftChanged(AiDraft? value)
    {
        StatusMessage = null;
        EditedText = value is null
            ? string.Empty
            : string.IsNullOrWhiteSpace(value.UserEditedText) ? value.DraftText : value.UserEditedText!;
    }

    [RelayCommand]
    private async Task LoadConditionsAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        IsLoadingConditions = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var conditions = await _conditionApiClient.GetConditionsAsync(workspaceGuid);

            Conditions.Clear();

            foreach (var condition in conditions)
            {
                Conditions.Add(condition);
            }

            SelectedCondition ??= Conditions.FirstOrDefault();

            OnPropertyChanged(nameof(HasConditions));
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading conditions. Please try again.";
        }
        finally
        {
            IsLoadingConditions = false;
            HasLoadedConditionsOnce = true;

            OnPropertyChanged(nameof(ShowNoConditionsState));
        }
    }

    [RelayCommand]
    private async Task LoadDraftsAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            return;
        }

        var previouslySelectedId = SelectedDraft?.Id;

        IsLoadingDrafts = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var drafts = await _aiDraftApiClient.GetConditionAiDraftsAsync(workspaceGuid, SelectedCondition.Id);

            Drafts.Clear();

            foreach (var draft in drafts)
            {
                Drafts.Add(draft);
            }

            OnPropertyChanged(nameof(HasDrafts));

            if (previouslySelectedId is { } id)
            {
                SelectedDraft = Drafts.FirstOrDefault(draft => draft.Id == id);
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
            ErrorMessage = "Something went wrong loading AI drafts. Please try again.";
        }
        finally
        {
            IsLoadingDrafts = false;
            HasLoadedDraftsOnce = true;

            OnPropertyChanged(nameof(ShowEmptyDraftsState));
        }
    }

    [RelayCommand]
    private Task SaveEditsAsync() => ApplyUpdateAsync(
        new UpdateAiDraftRequest { UserEditedText = EditedText, ReviewStatus = "USER_EDITED" },
        "Your edits were saved.");

    [RelayCommand]
    private Task ApproveAsync() => ApplyUpdateAsync(
        new UpdateAiDraftRequest { UserEditedText = EditedText, ReviewStatus = "APPROVED" },
        "Draft marked as approved.");

    [RelayCommand]
    private Task RejectAsync() => ApplyUpdateAsync(
        new UpdateAiDraftRequest { ReviewStatus = "REJECTED" },
        "Draft marked as rejected.");

    [RelayCommand]
    private Task SendBackToReviewAsync() => ApplyUpdateAsync(
        new UpdateAiDraftRequest { ReviewStatus = "USER_REVIEW_REQUIRED" },
        "Draft sent back to needs-review.");

    private async Task ApplyUpdateAsync(UpdateAiDraftRequest request, string successMessage)
    {
        if (SelectedDraft is null || !Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        IsSaving = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = null;

        try
        {
            await _aiDraftApiClient.UpdateAiDraftAsync(workspaceGuid, SelectedDraft.Id, request);
            await LoadDraftsAsync();

            StatusMessage = successMessage;
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not update this AI draft. Please try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ArchiveDraftAsync()
    {
        if (SelectedDraft is null || !Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync(
            "Archive draft",
            $"Archive the \"{SelectedDraft.DraftTypeLabel}\" draft? It will be hidden from this list and excluded from generated packs. This does not contact DVA.",
            "Archive",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        IsSaving = true;

        try
        {
            await _aiDraftApiClient.ArchiveAiDraftAsync(workspaceGuid, SelectedDraft.Id);
            SelectedDraft = null;
            await LoadDraftsAsync();
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not archive this AI draft. Please try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }
}
