namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class EvidenceGap
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid ConditionId { get; set; }

    public string GapType { get; set; } = string.Empty;

    public string GapStatus { get; set; } = "OPEN";

    public string Severity { get; set; } = "MEDIUM";

    public string PlainEnglishExplanation { get; set; } = string.Empty;

    public string? SuggestedNextStep { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
