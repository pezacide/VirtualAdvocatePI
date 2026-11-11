using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Data.Evidence;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Models.Evidence;
using VirtualAdvocatePI.Mobile.Services.Api;
using VirtualAdvocatePI.Mobile.Services.Dialogs;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class EvidenceViewModel : ObservableObject
{
    private readonly IConditionApiClient _conditionApiClient;
    private readonly IEvidenceApiClient _evidenceApiClient;
    private readonly IDialogService _dialogService;

    private bool _checklistLoaded;

    public EvidenceViewModel(
        IConditionApiClient conditionApiClient,
        IEvidenceApiClient evidenceApiClient,
        IDialogService dialogService)
    {
        _conditionApiClient = conditionApiClient;
        _evidenceApiClient = evidenceApiClient;
        _dialogService = dialogService;

        WorkspaceId = string.Empty;
        SelectedEvidenceType = EvidenceTypeOptions[0];
        DocumentDate = DateTime.Today;
    }

    public IReadOnlyList<EvidenceTypeOption> EvidenceTypeOptions { get; } = EvidenceTypeOption.All;

    public ObservableCollection<EvidenceChecklistGroupState> ChecklistGroups { get; } = new();

    public ObservableCollection<ClaimCondition> Conditions { get; } = new();

    public ObservableCollection<EvidenceItem> EvidenceItems { get; } = new();

    public ObservableCollection<EvidenceGap> EvidenceGaps { get; } = new();

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
    [NotifyPropertyChangedFor(nameof(ShowEmptyEvidenceState))]
    public partial bool IsLoadingEvidence { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyEvidenceState))]
    public partial bool HasLoadedEvidenceOnce { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingGaps { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? FormErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? FormStatusMessage { get; set; }

    [ObservableProperty]
    public partial bool IsSubmitting { get; set; }

    [ObservableProperty]
    public partial bool IsUploading { get; set; }

    [ObservableProperty]
    public partial string? UploadStatusMessage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusyWithItem))]
    public partial Guid? BusyEvidenceItemId { get; set; }

    // Add-evidence-item / upload form fields
    [ObservableProperty]
    public partial EvidenceTypeOption SelectedEvidenceType { get; set; }

    [ObservableProperty]
    public partial bool HasKnownDocumentDate { get; set; }

    [ObservableProperty]
    public partial DateTime DocumentDate { get; set; }

    [ObservableProperty]
    public partial string? ProviderName { get; set; }

    [ObservableProperty]
    public partial string? Notes { get; set; }

    public bool IsBusyWithItem => BusyEvidenceItemId.HasValue;

    public bool HasConditions => Conditions.Count > 0;

    public bool ShowNoConditionsState => HasLoadedConditionsOnce && !IsLoadingConditions && !HasConditions;

    public bool HasEvidenceItems => EvidenceItems.Count > 0;

    public bool ShowEmptyEvidenceState =>
        HasLoadedEvidenceOnce && !IsLoadingEvidence && SelectedCondition is not null && !HasEvidenceItems;

    public bool HasEvidenceGaps => EvidenceGaps.Count > 0;

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadChecklistState();
        LoadConditionsCommand.ExecuteAsync(null);
    }

    partial void OnSelectedConditionChanged(ClaimCondition? value)
    {
        if (value is not null)
        {
            LoadEvidenceCommand.ExecuteAsync(null);
        }
    }

    private void LoadChecklistState()
    {
        if (_checklistLoaded)
        {
            return;
        }

        _checklistLoaded = true;

        var checkedLabels = new HashSet<string>(
            Preferences.Default.Get(ChecklistPreferenceKey(), string.Empty)
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            StringComparer.Ordinal);

        ChecklistGroups.Clear();

        foreach (var group in EvidencePreparationChecklist.Groups)
        {
            var items = group.Items
                .Select(label =>
                {
                    var item = new EvidenceChecklistItemState
                    {
                        Label = label,
                        IsChecked = checkedLabels.Contains(label),
                    };

                    item.ChangedCallback = SaveChecklistState;

                    return item;
                })
                .ToList();

            ChecklistGroups.Add(new EvidenceChecklistGroupState
            {
                Title = group.Title,
                Description = group.Description,
                Items = items,
            });
        }
    }

    private void SaveChecklistState()
    {
        var checkedLabels = ChecklistGroups
            .SelectMany(group => group.Items)
            .Where(item => item.IsChecked)
            .Select(item => item.Label);

        Preferences.Default.Set(ChecklistPreferenceKey(), string.Join('|', checkedLabels));
    }

    private string ChecklistPreferenceKey() => $"evidence-checklist:{WorkspaceId}";

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
    private async Task LoadEvidenceAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            return;
        }

        IsLoadingEvidence = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var items = await _evidenceApiClient.GetConditionEvidenceItemsAsync(workspaceGuid, SelectedCondition.Id);

            EvidenceItems.Clear();

            foreach (var item in items)
            {
                EvidenceItems.Add(item);
            }

            OnPropertyChanged(nameof(HasEvidenceItems));

            await LoadGapsAsync();
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading evidence for this condition. Please try again.";
        }
        finally
        {
            IsLoadingEvidence = false;
            HasLoadedEvidenceOnce = true;

            OnPropertyChanged(nameof(ShowEmptyEvidenceState));
        }
    }

    private async Task LoadGapsAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            return;
        }

        IsLoadingGaps = true;

        try
        {
            var gaps = await _evidenceApiClient.GetConditionEvidenceGapsAsync(workspaceGuid, SelectedCondition.Id);

            EvidenceGaps.Clear();

            foreach (var gap in gaps)
            {
                EvidenceGaps.Add(gap);
            }

            OnPropertyChanged(nameof(HasEvidenceGaps));
        }
        catch (Exception)
        {
            // Gap loading is secondary to the evidence list; surface nothing loud here.
        }
        finally
        {
            IsLoadingGaps = false;
        }
    }

    [RelayCommand]
    private async Task RecalculateGapsAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            return;
        }

        IsLoadingGaps = true;

        try
        {
            var gaps = await _evidenceApiClient.RecalculateEvidenceGapsAsync(workspaceGuid, SelectedCondition.Id);

            EvidenceGaps.Clear();

            foreach (var gap in gaps)
            {
                EvidenceGaps.Add(gap);
            }

            OnPropertyChanged(nameof(HasEvidenceGaps));
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not recalculate evidence gaps. Please try again.";
        }
        finally
        {
            IsLoadingGaps = false;
        }
    }

    [RelayCommand]
    private async Task AddEvidenceItemAsync()
    {
        FormErrorMessage = null;
        FormStatusMessage = null;

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            FormErrorMessage = "Select a condition first.";
            return;
        }

        IsSubmitting = true;

        try
        {
            await _evidenceApiClient.CreateEvidenceItemAsync(workspaceGuid, SelectedCondition.Id, new CreateEvidenceItemRequest
            {
                EvidenceType = SelectedEvidenceType.Value,
                EvidenceStatus = "LISTED_NOT_UPLOADED",
                DocumentDate = HasKnownDocumentDate ? DateOnly.FromDateTime(DocumentDate) : null,
                ProviderName = string.IsNullOrWhiteSpace(ProviderName) ? null : ProviderName!.Trim(),
                UserNotes = string.IsNullOrWhiteSpace(Notes) ? null : Notes!.Trim(),
            });

            ResetForm();
            FormStatusMessage = "Evidence item added to the list.";

            await LoadEvidenceAsync();
        }
        catch (ApiRequestException ex)
        {
            FormErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            FormErrorMessage = "Could not add the evidence item. Please try again.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    [RelayCommand]
    private async Task UploadFileAsync()
    {
        FormErrorMessage = null;
        FormStatusMessage = null;
        UploadStatusMessage = null;

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            FormErrorMessage = "Select a condition first.";
            return;
        }

        FileResult? picked;

        try
        {
            picked = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose a document to upload",
            });
        }
        catch (Exception)
        {
            FormErrorMessage = "Could not open the file picker on this device.";
            return;
        }

        if (picked is null)
        {
            return;
        }

        var contentType = ResolveContentType(picked);

        IsUploading = true;

        try
        {
            await using var readStream = await picked.OpenReadAsync();
            var fileSize = readStream.CanSeek ? readStream.Length : (long?)null;

            var upload = await _evidenceApiClient.CreateEvidenceUploadUrlAsync(workspaceGuid, SelectedCondition.Id, new CreateEvidenceUploadUrlRequest
            {
                EvidenceType = SelectedEvidenceType.Value,
                OriginalFileName = picked.FileName,
                FileType = contentType,
                FileSize = fileSize,
                DocumentDate = HasKnownDocumentDate ? DateOnly.FromDateTime(DocumentDate) : null,
                ProviderName = string.IsNullOrWhiteSpace(ProviderName) ? null : ProviderName!.Trim(),
                UserNotes = string.IsNullOrWhiteSpace(Notes) ? null : Notes!.Trim(),
            });

            await _evidenceApiClient.UploadFileToSignedUrlAsync(upload.Upload!.Url, readStream, contentType);
            await _evidenceApiClient.MarkEvidenceUploadedAsync(workspaceGuid, upload.EvidenceItem!.Id);

            ResetForm();
            UploadStatusMessage = $"Uploaded {picked.FileName}.";

            await LoadEvidenceAsync();
        }
        catch (ApiRequestException ex)
        {
            FormErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            FormErrorMessage = "The file could not be uploaded. Please try again.";
        }
        finally
        {
            IsUploading = false;
        }
    }

    [RelayCommand]
    private async Task OpenFileAsync(EvidenceItem? item)
    {
        if (item is null || !item.IsUploaded)
        {
            return;
        }

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        BusyEvidenceItemId = item.Id;

        try
        {
            var download = await _evidenceApiClient.CreateEvidenceDownloadUrlAsync(workspaceGuid, item.Id);

            await Browser.Default.OpenAsync(download.Url, BrowserLaunchMode.SystemPreferred);
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not open this file. Please try again.";
        }
        finally
        {
            BusyEvidenceItemId = null;
        }
    }

    [RelayCommand]
    private async Task RemoveEvidenceItemAsync(EvidenceItem? item)
    {
        if (item is null || !Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        var confirmed = await _dialogService.ConfirmAsync(
            "Remove evidence item",
            $"Remove \"{item.EvidenceTypeLabel}\" from this workspace? This hides it from the evidence list, evidence gaps and generated packs. It does not contact DVA and does not delete anything already submitted outside this app.",
            "Remove",
            "Cancel");

        if (!confirmed)
        {
            return;
        }

        BusyEvidenceItemId = item.Id;

        try
        {
            await _evidenceApiClient.ArchiveEvidenceItemAsync(workspaceGuid, item.Id);
            await LoadEvidenceAsync();
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Could not remove this evidence item. Please try again.";
        }
        finally
        {
            BusyEvidenceItemId = null;
        }
    }

    private void ResetForm()
    {
        SelectedEvidenceType = EvidenceTypeOptions[0];
        HasKnownDocumentDate = false;
        DocumentDate = DateTime.Today;
        ProviderName = null;
        Notes = null;
    }

    private static string ResolveContentType(FileResult file)
    {
        if (!string.IsNullOrWhiteSpace(file.ContentType) && file.ContentType.Contains('/'))
        {
            return file.ContentType;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".txt" => "text/plain",
            ".rtf" => "application/rtf",
            _ => "application/octet-stream",
        };
    }
}
