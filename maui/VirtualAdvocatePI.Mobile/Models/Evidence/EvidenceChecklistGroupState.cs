namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceChecklistGroupState
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required IReadOnlyList<EvidenceChecklistItemState> Items { get; init; }
}
