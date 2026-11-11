namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceTypeOption
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public override string ToString() => Label;

    // Values must match GetAllowedEvidenceTypes() in the backend
    // EvidenceAndAuditEndpoints.
    public static IReadOnlyList<EvidenceTypeOption> All { get; } = new List<EvidenceTypeOption>
    {
        new() { Value = "DVA_DECISION_LETTER", Label = "DVA decision letter" },
        new() { Value = "PREVIOUS_PI_ASSESSMENT", Label = "Previous PI assessment" },
        new() { Value = "DCP_ASSESSMENT", Label = "DCP assessment" },
        new() { Value = "MEDICAL_REPORT", Label = "Medical report / GP report" },
        new() { Value = "SPECIALIST_REPORT", Label = "Specialist report" },
        new() { Value = "IMAGING_REPORT", Label = "Imaging or test report" },
        new() { Value = "MEDICATION_LIST", Label = "Medication list" },
        new() { Value = "TREATMENT_SUMMARY", Label = "Treatment summary" },
        new() { Value = "SERVICE_DOCUMENT", Label = "Service document" },
        new() { Value = "PERSONAL_STATEMENT", Label = "Personal statement" },
        new() { Value = "FUNCTIONAL_IMPACT_NOTES", Label = "Functional impact notes" },
        new() { Value = "APPOINTMENT_NOTES", Label = "Appointment notes" },
        new() { Value = "OTHER", Label = "Other" },
    };

    public static string LabelFor(string value) =>
        All.FirstOrDefault(option => string.Equals(option.Value, value, StringComparison.OrdinalIgnoreCase))?.Label
        ?? value;
}
