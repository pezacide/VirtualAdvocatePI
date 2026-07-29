namespace VirtualAdvocatePI.Mobile.Models.Conditions;

public sealed class CreateConditionRequest
{
    public string? ConditionName { get; init; }

    public string? DiagnosisStatus { get; init; }

    public DateOnly? DateDiagnosed { get; init; }

    public string? CurrentSymptoms { get; init; }

    public string? TreatmentSummary { get; init; }

    public string? MedicationSummary { get; init; }

    public string? FunctionalImpactSummary { get; init; }

    public bool? IsPrimaryCondition { get; init; }
}
