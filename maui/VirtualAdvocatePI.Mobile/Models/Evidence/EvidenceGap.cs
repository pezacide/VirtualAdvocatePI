namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceGap
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public Guid? ConditionId { get; init; }

    public string GapType { get; init; } = string.Empty;

    public string GapStatus { get; init; } = string.Empty;

    public string Severity { get; init; } = string.Empty;

    public string PlainEnglishExplanation { get; init; } = string.Empty;

    public string? SuggestedNextStep { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public string SeverityLabel => Severity switch
    {
        "HIGH" => "High priority",
        "MEDIUM" => "Medium priority",
        "LOW" => "Low priority",
        _ => Severity
    };

    public string GapTypeLabel => string.Join(
        ' ',
        GapType.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Length == 0
                ? part
                : char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));

    public bool HasSuggestedNextStep => !string.IsNullOrWhiteSpace(SuggestedNextStep);
}
