using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VirtualAdvocatePI.Mobile.Data.GarpM;
using VirtualAdvocatePI.Mobile.Models.Conditions;
using VirtualAdvocatePI.Mobile.Models.GarpM;
using VirtualAdvocatePI.Mobile.Models.QuestionResponses;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.ViewModels;

[QueryProperty(nameof(WorkspaceId), "workspaceId")]
public partial class GarpMStructuredSummaryViewModel : ObservableObject
{
    private readonly IConditionApiClient _conditionApiClient;
    private readonly IQuestionResponseApiClient _questionResponseApiClient;

    private static readonly IReadOnlyList<GarpMQuestionTemplate> AllQuestions =
        GarpMQuestionGroups.ActiveGroups.SelectMany(group => group.Questions).ToList();

    private List<QuestionResponse> _savedResponses = new();

    public GarpMStructuredSummaryViewModel(
        IConditionApiClient conditionApiClient,
        IQuestionResponseApiClient questionResponseApiClient)
    {
        _conditionApiClient = conditionApiClient;
        _questionResponseApiClient = questionResponseApiClient;

        WorkspaceId = string.Empty;
        PlainEnglishSummary = string.Empty;
    }

    public string SafetyBoundary => GarpMSafetyBoundary.Text;

    public ObservableCollection<ClaimCondition> Conditions { get; } = new();

    public ObservableCollection<GarpMSummarySection> Sections { get; } = new();

    public ObservableCollection<GarpMQuestionTemplate> MissingRequiredQuestions { get; } = new();

    [ObservableProperty]
    public partial string WorkspaceId { get; set; }

    [ObservableProperty]
    public partial ClaimCondition? SelectedCondition { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool IsLoadingConditions { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingResponses { get; set; }

    [ObservableProperty]
    public partial bool HasError { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial string? CopyStatusMessage { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNoConditionsState))]
    public partial bool HasLoadedConditionsOnce { get; set; }

    [ObservableProperty]
    public partial int SavedAnswersCount { get; set; }

    [ObservableProperty]
    public partial int TotalQuestionsCount { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMissingRequiredQuestions))]
    public partial int MissingRequiredCount { get; set; }

    [ObservableProperty]
    public partial string LastSavedAtText { get; set; } = "Not saved yet";

    [ObservableProperty]
    public partial string PlainEnglishSummary { get; set; }

    public bool HasConditions => Conditions.Count > 0;

    public bool HasMissingRequiredQuestions => MissingRequiredCount > 0;

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

            BuildSummary();
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
    private async Task CopySummaryAsync()
    {
        CopyStatusMessage = null;

        try
        {
            await Clipboard.Default.SetTextAsync(PlainEnglishSummary);
            CopyStatusMessage = "Copied to clipboard. You can now paste it into notes, an email, or another document.";
        }
        catch (Exception)
        {
            CopyStatusMessage = "Could not copy automatically. You can still select and copy the summary text manually.";
        }
    }

    private void BuildSummary()
    {
        var latestByQuestionId = _savedResponses
            .GroupBy(response => response.QuestionKey.Replace("garp_m:", string.Empty, StringComparison.Ordinal))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(response => response.UpdatedAt).Last());

        Sections.Clear();
        MissingRequiredQuestions.Clear();

        foreach (var group in GarpMQuestionGroups.ActiveGroups)
        {
            var answers = new List<GarpMSummaryAnswer>();
            var missingRequired = new List<GarpMQuestionTemplate>();

            foreach (var question in group.Questions)
            {
                if (latestByQuestionId.TryGetValue(question.Id, out var response) &&
                    !string.IsNullOrWhiteSpace(response.AnswerText))
                {
                    answers.Add(new GarpMSummaryAnswer
                    {
                        Question = question,
                        AnswerText = FormatAnswer(question, response.AnswerText!),
                    });
                }
                else if (question.IsRequired)
                {
                    missingRequired.Add(question);
                    MissingRequiredQuestions.Add(question);
                }
            }

            Sections.Add(new GarpMSummarySection
            {
                Title = group.Title,
                Description = group.Description,
                Answers = answers,
                MissingRequired = missingRequired,
            });
        }

        SavedAnswersCount = latestByQuestionId.Count;
        TotalQuestionsCount = AllQuestions.Count;
        MissingRequiredCount = MissingRequiredQuestions.Count;

        var latestSavedAt = _savedResponses.Select(response => response.UpdatedAt).OrderDescending().FirstOrDefault();
        LastSavedAtText = latestSavedAt == default ? "Not saved yet" : latestSavedAt.LocalDateTime.ToString("dd MMM yyyy, h:mm tt");

        PlainEnglishSummary = BuildPlainEnglishSummary();
    }

    private static string FormatAnswer(GarpMQuestionTemplate question, string answerText)
    {
        if (question.Options is null || question.Options.Count == 0)
        {
            return answerText;
        }

        var optionsByValue = question.Options.ToDictionary(option => option.Value, option => option.Label);
        var parts = answerText.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length <= 1)
        {
            return optionsByValue.TryGetValue(answerText, out var label) ? label : answerText;
        }

        return string.Join(", ", parts.Select(part => optionsByValue.TryGetValue(part, out var label) ? label : part));
    }

    private string BuildPlainEnglishSummary()
    {
        var lines = new StringBuilder();

        lines.AppendLine("GARP M-aware preparation summary");
        lines.AppendLine();
        lines.AppendLine($"Condition: {SelectedCondition?.ConditionName ?? "Selected condition"}");
        lines.AppendLine($"Last saved: {LastSavedAtText}");
        lines.AppendLine();
        lines.AppendLine("Important boundary:");
        lines.AppendLine("This summary is preparation support only. It does not calculate GARP M impairment points, estimate compensation, provide legal advice, provide medical advice, make a DVA decision, or guarantee a claim outcome.");
        lines.AppendLine();

        foreach (var section in Sections)
        {
            lines.AppendLine(section.Title);
            lines.AppendLine(new string('-', section.Title.Length));

            if (!section.HasAnswers)
            {
                lines.AppendLine("No saved answers yet.");
                lines.AppendLine();
                continue;
            }

            foreach (var answer in section.Answers)
            {
                lines.AppendLine($"{answer.Question.SummaryLabel}:");
                lines.AppendLine(string.IsNullOrWhiteSpace(answer.AnswerText) ? "No answer recorded." : answer.AnswerText);
                lines.AppendLine();
            }
        }

        lines.AppendLine("Missing required answers");
        lines.AppendLine("------------------------");

        if (MissingRequiredQuestions.Count == 0)
        {
            lines.AppendLine("No required answers are currently missing.");
        }
        else
        {
            foreach (var question in MissingRequiredQuestions)
            {
                lines.AppendLine($"- {question.QuestionText}");
            }
        }

        lines.AppendLine();
        lines.AppendLine("Suggested next step:");
        lines.AppendLine("Review this summary, correct anything that is incomplete or unclear, and discuss relevant points with a doctor, advocate, lawyer or support person where appropriate.");

        return lines.ToString().TrimEnd();
    }
}
