namespace VirtualAdvocatePI.Mobile.Models.Conditions;

public sealed class DiagnosisStatusOption
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public override string ToString() => Label;

    public static IReadOnlyList<DiagnosisStatusOption> All { get; } = new List<DiagnosisStatusOption>
    {
        new() { Value = "DIAGNOSED", Label = "Diagnosed" },
        new() { Value = "SUSPECTED", Label = "Suspected / being investigated" },
        new() { Value = "UNSURE", Label = "Unsure" },
        new() { Value = "NOT_DIAGNOSED", Label = "Not diagnosed" },
    };
}
