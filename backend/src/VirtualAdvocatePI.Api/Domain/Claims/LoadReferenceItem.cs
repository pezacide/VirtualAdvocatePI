namespace VirtualAdvocatePI.Api.Domain.Claims;

/// <summary>
/// A reference weight for a piece of RAN/ADF equipment or field store, used as a
/// lookup while completing the load exposure builder. Reference guidance only -
/// approximate typical weights, admin-editable, not an authoritative source.
/// </summary>
public sealed class LoadReferenceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string ItemName { get; set; } = string.Empty;

    public string Category { get; set; } = "OTHER";

    public decimal? TypicalWeightKg { get; set; }

    public string? WeightRangeKg { get; set; }

    public string? ServiceContext { get; set; }

    public string? Notes { get; set; }

    public string SourceLabel { get; set; } = "Virtual Advocate PI starter reference set";

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
