namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class AcceptedConditionHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid ConditionId { get; set; }

    public string PreviouslyAcceptedByDva { get; set; } = "UNSURE";

    public string OriginalAct { get; set; } = "UNKNOWN";

    public string PreviousCompensationReceived { get; set; } = "UNSURE";

    public string PreviousDvaDecisionLetterAvailable { get; set; } = "UNSURE";

    public string PreviousAssessmentLetterAvailable { get; set; } = "UNSURE";

    public DateOnly? PreviousDecisionDate { get; set; }

    public DateOnly? PreviousAssessmentDate { get; set; }

    public string WorseningClaimed { get; set; } = "UNSURE";

    public string? WorseningSummary { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
