using CommunityToolkit.Mvvm.ComponentModel;
using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.ViewModels;

public partial class GarpMAnswerItem : ObservableObject
{
    public GarpMAnswerItem(GarpMQuestionTemplate question)
    {
        Question = question;
        AvailableOptions = question.Options ?? new List<GarpMQuestionOption>();

        IsTextAnswerType = question.AnswerType == GarpMAnswerTypes.Text;
        IsLongTextAnswerType = question.AnswerType == GarpMAnswerTypes.LongText;
        IsDateAnswerType = question.AnswerType == GarpMAnswerTypes.Date;
        IsSelectAnswerType =
            question.AnswerType == GarpMAnswerTypes.SingleSelect ||
            question.AnswerType == GarpMAnswerTypes.YesNo ||
            question.AnswerType == GarpMAnswerTypes.YesNoUnsure;

        DateValue = DateTime.Today;
    }

    public GarpMQuestionTemplate Question { get; }

    public IReadOnlyList<GarpMQuestionOption> AvailableOptions { get; }

    public bool IsTextAnswerType { get; }

    public bool IsLongTextAnswerType { get; }

    public bool IsDateAnswerType { get; }

    public bool IsSelectAnswerType { get; }

    [ObservableProperty]
    public partial string? TextValue { get; set; }

    [ObservableProperty]
    public partial bool HasDateValue { get; set; }

    [ObservableProperty]
    public partial DateTime DateValue { get; set; }

    [ObservableProperty]
    public partial GarpMQuestionOption? SelectedOption { get; set; }

    [ObservableProperty]
    public partial bool IsSaved { get; set; }

    [ObservableProperty]
    public partial DateTimeOffset? SavedAt { get; set; }

    public bool IsEmpty()
    {
        if (IsDateAnswerType)
        {
            return !HasDateValue;
        }

        if (IsSelectAnswerType)
        {
            return SelectedOption is null;
        }

        return string.IsNullOrWhiteSpace(TextValue);
    }

    public bool IsMissingRequired() => Question.IsRequired && IsEmpty();

    public string? GetAnswerText()
    {
        if (IsDateAnswerType)
        {
            return HasDateValue ? DateOnly.FromDateTime(DateValue).ToString("yyyy-MM-dd") : null;
        }

        if (IsSelectAnswerType)
        {
            return SelectedOption?.Value;
        }

        return TextValue;
    }

    public void SetAnswerText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (IsDateAnswerType)
        {
            if (DateOnly.TryParse(value, out var date))
            {
                HasDateValue = true;
                DateValue = date.ToDateTime(TimeOnly.MinValue);
            }

            return;
        }

        if (IsSelectAnswerType)
        {
            SelectedOption = AvailableOptions.FirstOrDefault(option => option.Value == value);
            return;
        }

        TextValue = value;
    }

    public string? GetValidationMessage()
    {
        if (IsMissingRequired())
        {
            return "This answer is required before the section can be considered complete.";
        }

        if (Question.ValidationRules is null)
        {
            return null;
        }

        var text = (GetAnswerText() ?? string.Empty).Trim();

        foreach (var rule in Question.ValidationRules)
        {
            if (rule.Type == "MIN_LENGTH" && text.Length > 0 && text.Length < (rule.Value ?? 0))
            {
                return rule.Message ?? $"Enter at least {rule.Value} characters.";
            }

            if (rule.Type == "MAX_LENGTH" && (rule.Value ?? 0) > 0 && text.Length > rule.Value)
            {
                return rule.Message ?? $"Keep this answer under {rule.Value} characters.";
            }
        }

        return null;
    }
}
