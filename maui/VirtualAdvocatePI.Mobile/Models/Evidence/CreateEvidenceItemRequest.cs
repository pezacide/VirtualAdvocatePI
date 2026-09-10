namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class CreateEvidenceItemRequest
{
    public string? EvidenceType { get; init; }

    public string? EvidenceStatus { get; init; }

    public DateOnly? DocumentDate { get; init; }

    public string? ProviderName { get; init; }

    public string? UserNotes { get; init; }
}
