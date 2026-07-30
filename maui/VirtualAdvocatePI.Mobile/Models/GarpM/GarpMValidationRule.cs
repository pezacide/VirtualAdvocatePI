namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMValidationRule
{
    public required string Type { get; init; }

    public int? Value { get; init; }

    public string? Message { get; init; }
}
