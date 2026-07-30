namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMQuestionOption
{
    public required string Value { get; init; }

    public required string Label { get; init; }

    public string? HelperText { get; init; }

    public override string ToString() => Label;
}
