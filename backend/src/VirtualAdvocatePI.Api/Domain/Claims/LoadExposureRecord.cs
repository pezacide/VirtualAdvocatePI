namespace VirtualAdvocatePI.Api.Domain.Claims;

/// <summary>
/// A single record of physically demanding service exposure (lifting/carrying,
/// stairs and ladders, kneeling/squatting, neck and shoulder load carriage,
/// heavy load carrying, or a hazard exposure). Workspace-scoped; optionally
/// linked to a specific condition. Preparation support only - this is not an
/// impairment calculation or a compensation estimate.
/// </summary>
public sealed class LoadExposureRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid? ConditionId { get; set; }

    public string RecordType { get; set; } = "LIFTING_CARRYING";

    public string ActivityDescription { get; set; } = string.Empty;

    public string BodyAreaAffected { get; set; } = "MULTIPLE";

    public decimal? TypicalWeightKg { get; set; }

    public decimal? MaxWeightKg { get; set; }

    public string Frequency { get; set; } = "UNSURE";

    public string? DurationPerOccasion { get; set; }

    public string? RepetitionsDescription { get; set; }

    public DateOnly? ServicePeriodFrom { get; set; }

    public DateOnly? ServicePeriodTo { get; set; }

    public decimal? YearsExposed { get; set; }

    public string? EquipmentOrContext { get; set; }

    /// <summary>Only meaningful when <see cref="RecordType"/> is HAZARD_EXPOSURE.</summary>
    public string? HazardType { get; set; }

    public string? Notes { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
