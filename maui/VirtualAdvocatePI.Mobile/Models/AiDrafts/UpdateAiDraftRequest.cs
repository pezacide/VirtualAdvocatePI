namespace VirtualAdvocatePI.Mobile.Models.AiDrafts;

public sealed class UpdateAiDraftRequest
{
    public string? UserEditedText { get; init; }

    public string? ReviewStatus { get; init; }
}
