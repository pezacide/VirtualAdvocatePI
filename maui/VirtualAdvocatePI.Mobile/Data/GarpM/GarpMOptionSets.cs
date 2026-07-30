using VirtualAdvocatePI.Mobile.Models.GarpM;

namespace VirtualAdvocatePI.Mobile.Data.GarpM;

public static class GarpMOptionSets
{
    public static IReadOnlyList<GarpMQuestionOption> YesNoUnsure { get; } = new List<GarpMQuestionOption>
    {
        new() { Value = "YES", Label = "Yes" },
        new() { Value = "NO", Label = "No" },
        new() { Value = "UNSURE", Label = "Unsure" },
    };

    public static IReadOnlyList<GarpMQuestionOption> Stability { get; } = new List<GarpMQuestionOption>
    {
        new() { Value = "STABLE", Label = "Stable" },
        new() { Value = "IMPROVING", Label = "Improving" },
        new() { Value = "WORSENING", Label = "Worsening" },
        new() { Value = "FLUCTUATING", Label = "Fluctuating" },
        new() { Value = "UNSURE", Label = "Unsure" },
    };

    public static IReadOnlyList<GarpMQuestionOption> ImpactFrequency { get; } = new List<GarpMQuestionOption>
    {
        new() { Value = "DAILY", Label = "Daily" },
        new() { Value = "MOST_DAYS", Label = "Most days" },
        new() { Value = "WEEKLY", Label = "Weekly" },
        new() { Value = "OCCASIONAL", Label = "Occasional" },
        new() { Value = "FLARE_UPS_ONLY", Label = "During flare-ups only" },
        new() { Value = "UNSURE", Label = "Unsure" },
    };
}
