using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Data.GarpM;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Models.GarpM;
using VirtualAdvocatePI.Mobile.Models.QuestionResponses;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class GarpMQuestionEngineViewModel : ObservableObject
{
    private readonly IConditionApiClient _conditionApiClient;
    private readonly IQuestionResponseApiClient _questionResponseApiClient;

    private List<QuestionResponse> _savedResponses = new();

    public GarpMQuestionEngineViewModel(
        IConditionApiClient conditionApiClient,
        IQuestionResponseApiClient questionResponseApiClient)
    {
        _conditionApiClient = conditionApiClient;
        _questionResponseApiClient = questionResponseApiClient;

        WorkspaceId = string.Empty;
    }

    public string SafetyBoundary => GarpMSafetyBoundary.Text;

    public IReadOnlyList<GarpMQuestionGroupTemplate> ActiveGroups { get; } = GarpMQuestionGroups.ActiveGroups.ToList();

    public ObservableCollection<ClaimCondition> Conditions { get; } = new();

    public ObservableCollection<GarpMAnswerItem> CurrentAnswers { get; } = new();

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

    [ObservableProperty]
    public partial ClaimCondition? SelectedCondition { get; set; }

    [ObservableProperty]
    public partial GarpMQuestionGroupTemplate? SelectedGroup { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool IsLoadingConditions { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingResponses { get; set; }

    [ObservableProperty]
    public partial bool IsSaving { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? StatusMessage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool HasLoadedConditionsOnce { get; set; }

    [ObservableProperty]
    public partial int AnsweredCount { get; set; }

    [ObservableProperty]
    public partial int SavedCount { get; set; }

    [ObservableProperty]
    public partial int MissingRequiredCount { get; set; }

    public bool HasConditions => Conditions.Count > 0;

    public bool ShowNoConditionsState => HasLoadedConditionsOnce && !IsLoadingConditions && !HasConditions;

    partial void OnWorkspaceIdChanged(string value)
    {
        LoadConditionsCommand.ExecuteAsync(null);
    }

    partial void OnSelectedConditionChanged(ClaimCondition? value)
    {
        if (value is not null)
        {
            LoadResponsesCommand.ExecuteAsync(null);
        }
    }

    partial void OnSelectedGroupChanged(GarpMQuestionGroupTemplate? value)
    {
        BuildCurrentAnswers();
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
            SelectedGroup ??= ActiveGroups.FirstOrDefault();

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
    private async Task LoadResponsesAsync()
    {
        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            return;
        }

        IsLoadingResponses = true;
        HasError = false;
        ErrorMessage = null;

        try
        {
            var responses = await _questionResponseApiClient.GetQuestionResponsesAsync(workspaceGuid, SelectedCondition.Id);

            _savedResponses = responses
                .Where(response => response.QuestionKey.StartsWith("garp_m:", StringComparison.Ordinal))
                .ToList();

            BuildCurrentAnswers();
        }
        catch (ApiRequestException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Something went wrong loading saved answers. Please try again.";
        }
        finally
        {
            IsLoadingResponses = false;
        }
    }

    [RelayCommand]
    private async Task SaveSectionAsync()
    {
        StatusMessage = null;
        ErrorMessage = null;

        if (!Guid.TryParse(WorkspaceId, out var workspaceGuid) || SelectedCondition is null)
        {
            ErrorMessage = "Select a condition before saving question responses.";
            return;
        }

        var toSave = CurrentAnswers.Where(item => !item.IsEmpty()).ToList();

        if (toSave.Count == 0)
        {
            ErrorMessage = "Add at least one answer before saving this section.";
            return;
        }

        IsSaving = true;

        try
        {
            foreach (var item in toSave)
            {
                await _questionResponseApiClient.CreateQuestionResponseAsync(
                    workspaceGuid,
                    SelectedCondition.Id,
                    new CreateQuestionResponseRequest
                    {
                        QuestionGroup = GarpMQuestionMapper.ToBackendQuestionGroup(item.Question),
                        QuestionKey = item.Question.BackendQuestionKey,
                        QuestionText = item.Question.QuestionText,
                        AnswerText = item.GetAnswerText(),
                        AnswerType = item.Question.AnswerType
                    });
            }

            StatusMessage = $"Saved {toSave.Count} response(s) for this section.";

            await LoadResponsesAsync();
        }
        catch (ApiRequestException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            ErrorMessage = "Could not save answers. Please try again.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void BuildCurrentAnswers()
    {
        CurrentAnswers.Clear();

        if (SelectedGroup is null)
        {
            RecomputeProgress();
            return;
        }

        var latestByKey = _savedResponses
            .GroupBy(response => response.QuestionKey)
            .ToDictionary(group => group.Key, group => group.OrderBy(response => response.UpdatedAt).Last());

        foreach (var question in SelectedGroup.Questions.OrderBy(question => question.DisplayOrder))
        {
            var item = new GarpMAnswerItem(question);

            if (latestByKey.TryGetValue(question.BackendQuestionKey, out var response))
            {
                item.SetAnswerText(response.AnswerText);
                item.IsSaved = true;
                item.SavedAt = response.UpdatedAt;
            }

            CurrentAnswers.Add(item);
        }

        RecomputeProgress();
    }

    private void RecomputeProgress()
    {
        AnsweredCount = CurrentAnswers.Count(item => !item.IsEmpty());
        SavedCount = CurrentAnswers.Count(item => item.IsSaved);
        MissingRequiredCount = CurrentAnswers.Count(item => item.IsMissingRequired());
    }
}
