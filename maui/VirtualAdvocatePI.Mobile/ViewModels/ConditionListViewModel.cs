using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Dialogs;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class ConditionListViewModel : ObservableObject
{
    private readonly IConditionApiClient _conditionApiClient;
    private readonly IDialogService _dialogService;

    public ConditionListViewModel(
        IConditionApiClient conditionApiClient,
        IDialogService dialogService)
    {
        _conditionApiClient = conditionApiClient;
        _dialogService = dialogService;

        WorkspaceId = string.Empty;
        ConditionName = string.Empty;
        SelectedDiagnosisStatus = DiagnosisStatusOptions[0];
        DiagnosisDate = DateTime.Today;
    }

    public IReadOnlyList<DiagnosisStatusOption> DiagnosisStatusOptions { get; } = DiagnosisStatusOption.All;

    public ObservableCollection<ClaimCondition> Conditions { get; } = new();

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

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

    public bool HasConditions => Conditions.Count > 0;

    public bool ShowEmptyState => HasLoadedOnce && !IsLoading && !HasError && !HasConditions;

    [ObservableProperty]
    public partial string ConditionName { get; set; }

    [ObservableProperty]
    public partial DiagnosisStatusOption SelectedDiagnosisStatus { get; set; }

    [ObservableProperty]
    public partial bool HasKnownDiagnosisDate { get; set; }

    [ObservableProperty]
    public partial DateTime DiagnosisDate { get; set; }

    [ObservableProperty]
    public partial string? CurrentSymptoms { get; set; }

    [ObservableProperty]
    public partial string? TreatmentSummary { get; set; }

    [ObservableProperty]
    public partial string? MedicationSummary { get; set; }

    [ObservableProperty]
    public partial string? FunctionalImpactSummary { get; set; }

    [ObservableProperty]
    public partial bool IsPrimaryCondition { get; set; }

    [ObservableProperty]
    public partial bool IsSubmitting { get; set; }

    [ObservableProperty]
    public partial string? FormErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? FormStatusMessage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsArchiving))]
    public partial Guid? RemovingConditionId { get; set; }

    public bool IsArchiving => RemovingConditionId.HasValue;

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadConditionsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadConditionsAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        IsLoading = true;
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
            IsLoading = false;
            HasLoadedOnce = true;

            OnPropertyChanged(nameof(HasConditions));
            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    [RelayCommand]
    private async Task CreateConditionAsync()
    {
        FormErrorMessage = null;
        FormStatusMessage = null;

        if (string.IsNullOrWhiteSpace(ConditionName))
        {
            FormErrorMessage = "Enter a condition name.";
            return;
        }

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        IsSubmitting = true;

        try
        {
            await _conditionApiClient.CreateConditionAsync(workspaceGuid, new CreateConditionRequest
            {
                ConditionName = ConditionName.Trim(),
                DiagnosisStatus = SelectedDiagnosisStatus.Value,
                DateDiagnosed = HasKnownDiagnosisDate ? DateOnly.FromDateTime(DiagnosisDate) : null,
                CurrentSymptoms = string.IsNullOrWhiteSpace(CurrentSymptoms) ? null : CurrentSymptoms,
                TreatmentSummary = string.IsNullOrWhiteSpace(TreatmentSummary) ? null : TreatmentSummary,
                MedicationSummary = string.IsNullOrWhiteSpace(MedicationSummary) ? null : MedicationSummary,
                FunctionalImpactSummary = string.IsNullOrWhiteSpace(FunctionalImpactSummary) ? null : FunctionalImpactSummary,
                IsPrimaryCondition = IsPrimaryCondition
            });

            ConditionName = string.Empty;
            SelectedDiagnosisStatus = DiagnosisStatusOptions[0];
            HasKnownDiagnosisDate = false;
            DiagnosisDate = DateTime.Today;
            CurrentSymptoms = null;
            TreatmentSummary = null;
            MedicationSummary = null;
            FunctionalImpactSummary = null;
            IsPrimaryCondition = false;

            FormStatusMessage = "Condition added.";

            await LoadConditionsAsync();
        }
        catch (ApiRequestException ex)
        {
            FormErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            FormErrorMessage = "Could not add condition. Please try again.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private async Task ArchiveConditionAsync(ClaimCondition? condition)
    {
        if (condition is null)
        {
            return;
        }

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync(
            "Remove condition",
            $"Remove {condition.ConditionName} from this active workspace? This hides it from active condition lists, evidence upload, metadata, GARP M questions and evidence gaps. It does not contact DVA and does not delete anything already submitted outside this app.",
            "Remove",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        RemovingConditionId = condition.Id;

        try
        {
            await _conditionApiClient.ArchiveConditionAsync(workspaceGuid, condition.Id);
            await LoadConditionsAsync();
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not remove condition. Please try again.";
        }
        finally
        {
            RemovingConditionId = null;
        }
    }
}
