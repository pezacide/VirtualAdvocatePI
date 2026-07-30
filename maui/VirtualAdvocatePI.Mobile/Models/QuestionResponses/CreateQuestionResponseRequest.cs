namespace VirtualAdvocatePI.Mobile.Models.QuestionResponses;

public sealed class CreateQuestionResponseRequest
{
    public string? QuestionGroup { get; init; }

    public string? QuestionKey { get; init; }

    public string? QuestionText { get; init; }

    public string? AnswerText { get; init; }

    public string? AnswerType { get; init; }
}
