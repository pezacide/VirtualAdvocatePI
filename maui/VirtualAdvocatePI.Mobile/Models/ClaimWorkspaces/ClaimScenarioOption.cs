namespace VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;

public sealed class ClaimScenarioOption
{
    public required string Value { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public static IReadOnlyList<ClaimScenarioOption> All { get; } = new List<ClaimScenarioOption>
    {
        new()
        {
            Value = "NEW_CONDITION",
            Title = "New condition",
            Description = "Use this when preparing information for a condition that has not previously been accepted.",
        },
        new()
        {
            Value = "WORSENING_EXISTING_CONDITION",
            Title = "Worsening existing condition",
            Description = "Use this when an existing accepted condition may have worsened and needs evidence organised.",
        },
        new()
        {
            Value = "NEW_PLUS_EXISTING",
            Title = "New plus existing conditions",
            Description = "Use this when the preparation pack may involve both new and previously accepted conditions.",
        },
        new()
        {
            Value = "EVIDENCE_PREP_ONLY",
            Title = "Evidence preparation only",
            Description = "Use this when the main goal is organising documents, notes, gaps and questions before speaking with support.",
        },
        new()
        {
            Value = "UNSURE",
            Title = "Not sure yet",
            Description = "Use this when the pathway is unclear and the workspace should stay flexible.",
        },
    };
}
