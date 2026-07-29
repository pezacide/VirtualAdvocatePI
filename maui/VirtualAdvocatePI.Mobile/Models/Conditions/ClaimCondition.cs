namespace VirtualAdvocatePI.Mobile.Models.Conditions;

public sealed class ClaimCondition
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public string ConditionName { get; init; } = string.Empty;

    public string DiagnosisStatus { get; init; } = string.Empty;

    public DateOnly? DateDiagnosed { get; init; }

    public string? CurrentSymptoms { get; init; }

    public string? TreatmentSummary { get; init; }

    public string? MedicationSummary { get; init; }

    public string? MedicationSideEffects { get; init; }

    public string? FunctionalImpactSummary { get; init; }

    public string? LifestyleImpactSummary { get; init; }

    public string? WorkImpactSummary { get; init; }

    public string? StabilityNotes { get; init; }

    public string? WorseningNotes { get; init; }

    public bool IsPrimaryCondition { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }
}
