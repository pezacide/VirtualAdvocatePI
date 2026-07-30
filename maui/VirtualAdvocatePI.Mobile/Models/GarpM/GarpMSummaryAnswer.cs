namespace VirtualAdvocatePI.Mobile.Models.GarpM;

public sealed class GarpMSummaryAnswer
{
    public required GarpMQuestionTemplate Question { get; init; }

    public required string AnswerText { get; init; }
}
