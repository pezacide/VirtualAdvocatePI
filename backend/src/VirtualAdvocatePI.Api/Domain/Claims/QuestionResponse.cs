namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class QuestionResponse
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid ConditionId { get; set; }

    public string QuestionGroup { get; set; } = "CLAIM_CONTEXT";

    public string QuestionKey { get; set; } = string.Empty;

    public string QuestionText { get; set; } = string.Empty;

    public string? AnswerText { get; set; }

    public string AnswerType { get; set; } = "TEXT";

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
