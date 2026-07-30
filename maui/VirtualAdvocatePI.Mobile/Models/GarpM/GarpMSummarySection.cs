namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMSummarySection
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required IReadOnlyList<GarpMSummaryAnswer> Answers { get; init; }

    public required IReadOnlyList<GarpMQuestionTemplate> MissingRequired { get; init; }

    public int AnswerCount => Answers.Count;

    public bool HasAnswers => Answers.Count > 0;

    public bool HasMissingRequired => MissingRequired.Count > 0;
}
