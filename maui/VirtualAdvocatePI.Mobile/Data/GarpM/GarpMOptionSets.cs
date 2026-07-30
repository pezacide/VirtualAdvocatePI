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
}
