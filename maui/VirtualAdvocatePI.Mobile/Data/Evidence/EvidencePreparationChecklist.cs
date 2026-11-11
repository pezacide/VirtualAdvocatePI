namespace VirtualAdvocatePI.Mobile.Data.Evidence;

/// <summary>
/// Static evidence preparation checklist, ported from the web app's
/// EvidenceChecklistShell.tsx. This is preparation guidance only: it does not say
/// what DVA will require and it is not persisted server-side. Tick state is kept
/// per workspace in local <see cref="Preferences"/> only.
/// </summary>
public static class EvidencePreparationChecklist
{
    public sealed record ChecklistGroup(string Title, string Description, IReadOnlyList<string> Items);

    public static IReadOnlyList<ChecklistGroup> Groups { get; } = new List<ChecklistGroup>
    {
        new(
            "DVA history and previous decisions",
            "Useful when a condition has previously been accepted, assessed, compensated, reviewed or worsened.",
            new[]
            {
                "DVA decision letter",
                "Previous PI assessment letter",
                "DCP assessment or review material",
                "Previous compensation or payment correspondence",
                "Any letter showing the original Act, such as MRCA, DRCA or VEA",
            }),
        new(
            "Medical diagnosis and treatment",
            "Useful for showing the current clinical picture and what treatment has been tried.",
            new[]
            {
                "GP report or health summary",
                "Specialist report",
                "Diagnosis confirmation",
                "Treatment summary",
                "Medication list",
                "Medication side effect notes",
                "Imaging or test reports if relevant",
            }),
        new(
            "Functional and lifestyle impact",
            "Useful for describing how the condition affects ordinary daily activities.",
            new[]
            {
                "Personal statement notes",
                "Sleep impact notes",
                "Mobility or physical restriction notes",
                "Domestic task impact notes",
                "Social or relationship impact notes",
                "Work impact notes",
                "Flare-up or bad-day examples",
            }),
        new(
            "Service and connection notes",
            "Useful for organising the background information before speaking with an advocate, doctor, lawyer or support person.",
            new[]
            {
                "Service dates and role details",
                "Deployment, posting or workplace exposure notes",
                "Incident or exposure description",
                "Buddy statement or witness notes if available",
                "Timeline from service event to symptoms",
            }),
        new(
            "Appointment preparation",
            "Useful before seeing a GP, specialist, advocate, lawyer or support person.",
            new[]
            {
                "Questions for the doctor",
                "Symptoms to explain clearly",
                "Treatment changes to mention",
                "Evidence gaps to ask about",
                "Documents to request",
                "Follow-up actions after the appointment",
            }),
    };
}
