namespace VirtualAdvocatePI.Mobile.Models.QuestionResponses;

public sealed class QuestionResponse
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public Guid ConditionId { get; init; }

    public string QuestionGroup { get; init; } = string.Empty;

    public string QuestionKey { get; init; } = string.Empty;

    public string QuestionText { get; init; } = string.Empty;

    public string? AnswerText { get; init; }

    public string AnswerType { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
