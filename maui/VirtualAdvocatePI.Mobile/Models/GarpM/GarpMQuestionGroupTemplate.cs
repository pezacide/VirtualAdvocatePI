namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMQuestionGroupTemplate
{
    public required string GroupKey { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string WhyThisMatters { get; init; }

    public required int DisplayOrder { get; init; }

    public required string SafetyNote { get; init; }

    public required IReadOnlyList<GarpMQuestionTemplate> Questions { get; init; }
}
