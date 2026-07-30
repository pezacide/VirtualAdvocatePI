namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMQuestionTemplate
{
    public required string Id { get; init; }

    public required string GroupKey { get; init; }

    public required string QuestionKey { get; init; }

    public required string QuestionText { get; init; }

    public required string HelperText { get; init; }

    public required string AnswerType { get; init; }

    public required string RequirementLevel { get; init; }

    public required string EvidenceCategory { get; init; }

    public required int DisplayOrder { get; init; }

    public required string SummaryLabel { get; init; }

    public IReadOnlyList<GarpMQuestionOption>? Options { get; init; }

    public IReadOnlyList<GarpMValidationRule>? ValidationRules { get; init; }

    public string? SafetyNote { get; init; }

    public bool IsRequired => RequirementLevel == GarpMRequirementLevels.Required;

    public string BackendQuestionKey => $"garp_m:{Id}";
}
