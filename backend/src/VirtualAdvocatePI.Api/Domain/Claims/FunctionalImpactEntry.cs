namespace VirtualAdvocatePI.Api.Domain.Claims;

/// <summary>
/// A "Good day / bad day" functional impact entry for a condition: how an
/// activity domain is affected on a typical good day versus a bad day, how
/// often bad days occur, and what aids or help are used. Preparation support
/// only - this does not calculate impairment or capacity.
/// </summary>
public sealed class FunctionalImpactEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid ConditionId { get; set; }

    public string ActivityDomain { get; set; } = "OTHER";

    public string? GoodDayDescription { get; set; }

    public string? BadDayDescription { get; set; }

    public string BadDayFrequency { get; set; } = "UNSURE";

    public string? AidsOrHelpUsed { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
