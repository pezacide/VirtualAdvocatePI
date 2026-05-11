namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class ClaimCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public string ConditionName { get; set; } = string.Empty;

    public string DiagnosisStatus { get; set; } = "UNSURE";

    public DateOnly? DateDiagnosed { get; set; }

    public string? CurrentSymptoms { get; set; }

    public string? TreatmentSummary { get; set; }

    public string? MedicationSummary { get; set; }

    public string? MedicationSideEffects { get; set; }

    public string? FunctionalImpactSummary { get; set; }

    public string? LifestyleImpactSummary { get; set; }

    public string? WorkImpactSummary { get; set; }

    public string? StabilityNotes { get; set; }

    public string? WorseningNotes { get; set; }

    public bool IsPrimaryCondition { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
