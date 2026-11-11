using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class GeneratedDocumentsViewModel : ObservableObject
{
    private readonly IGeneratedDocumentApiClient _generatedDocumentApiClient;

    public GeneratedDocumentsViewModel(IGeneratedDocumentApiClient generatedDocumentApiClient)
    {
        _generatedDocumentApiClient = generatedDocumentApiClient;

        WorkspaceId = string.Empty;
    }

    public ObservableCollection<GeneratedDocument> Documents { get; } = new();

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool HasLoadedOnce { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsGenerating { get; set; }

    [ObservableProperty]
    public partial string? GenerateSummary { get; set; }

    [ObservableProperty]
    public partial string? GenerateError { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBusyWithDocument))]
    public partial Guid? BusyDocumentId { get; set; }

    public bool IsBusyWithDocument => BusyDocumentId.HasValue;

    public bool HasDocuments => Documents.Count > 0;

    public bool ShowEmptyState => HasLoadedOnce && !IsLoading && !HasError && !HasDocuments;

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadDocumentsCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task LoadDocumentsAsync()
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
            var documents = await _generatedDocumentApiClient.GetGeneratedDocumentsAsync(workspaceGuid);

            Documents.Clear();

            foreach (var document in documents)
            {
                Documents.Add(document);
            }

            OnPropertyChanged(nameof(HasDocuments));
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading generated documents. Please try again.";
        }
        finally
        {
            IsLoading = false;
            HasLoadedOnce = true;

            OnPropertyChanged(nameof(ShowEmptyState));
        }
    }

    [RelayCommand]
    private Task GenerateClaimStarterPackAsync() => GenerateAsync(
        workspaceGuid => _generatedDocumentApiClient.GenerateClaimStarterPackAsync(workspaceGuid),
        "Claim Starter Pack");

    [RelayCommand]
    private Task GenerateDoctorGuidancePackAsync() => GenerateAsync(
        workspaceGuid => _generatedDocumentApiClient.GenerateDoctorGuidancePackAsync(workspaceGuid),
        "Doctor Guidance Pack");

    private async Task GenerateAsync(
        Func<Guid, Task<GeneratePackResponse>> generate,
        string packName)
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        IsGenerating = true;
        GenerateSummary = null;
        GenerateError = null;

        try
        {
            var result = await generate(workspaceGuid);

            GenerateSummary = BuildSummary(packName, result);

            await LoadDocumentsAsync();
        }
        catch (ApiRequestException ex)
        {
            GenerateError = ex.Message;
        }
        catch (Exception)
        {
            GenerateError = $"Could not generate the {packName}. Please try again.";
        }
        finally
        {
            IsGenerating = false;
        }
    }

    private static string BuildSummary(string packName, GeneratePackResponse result)
    {
        var builder = new StringBuilder();

        builder.AppendLine($"{packName} generated.");

        if (!string.IsNullOrWhiteSpace(result.DocumentVersion))
        {
            builder.AppendLine($"Template version: {result.DocumentVersion}");
        }

        builder.AppendLine($"Active conditions: {result.ActiveConditionCount}");
        builder.AppendLine($"Evidence items: {result.EvidenceItemCount}");
        builder.AppendLine($"Evidence gaps: {result.EvidenceGapCount}");

        if (result.IncludedApprovedDoctorDraftCount > 0 || result.ExcludedUnapprovedDoctorDraftCount > 0)
        {
            builder.AppendLine($"Approved doctor drafts included: {result.IncludedApprovedDoctorDraftCount}");
            builder.AppendLine($"Unapproved doctor drafts excluded: {result.ExcludedUnapprovedDoctorDraftCount}");
        }
        else
        {
            builder.AppendLine($"Approved AI drafts included: {result.IncludedAiDraftCount}");
            builder.AppendLine($"Unapproved AI drafts excluded: {result.ExcludedUnapprovedAiDraftCount}");
        }

        if (!string.IsNullOrWhiteSpace(result.ReviewedOnlyRule))
        {
            builder.AppendLine();
            builder.AppendLine(result.ReviewedOnlyRule);
        }

        return builder.ToString().TrimEnd();
    }

    [RelayCommand]
    private Task DownloadDocxAsync(GeneratedDocument? document) => DownloadAsync(document, "DOCX");

    [RelayCommand]
    private Task DownloadPdfAsync(GeneratedDocument? document) => DownloadAsync(document, "PDF");

    private async Task DownloadAsync(GeneratedDocument? document, string format)
    {
        if (document is null || !Guid.TryParse(WorkspaceId, out var workspaceGuid))
        {
            return;
        }

        BusyDocumentId = document.Id;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var download = await _generatedDocumentApiClient.CreateDownloadUrlAsync(workspaceGuid, document.Id, format);

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
            ErrorMessage = $"Could not open the {format} download. Please try again.";
        }
        finally
        {
            BusyDocumentId = null;
        }
    }
}
