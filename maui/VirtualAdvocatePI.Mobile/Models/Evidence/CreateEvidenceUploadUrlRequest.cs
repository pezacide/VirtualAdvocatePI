namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class CreateEvidenceUploadUrlRequest
{
    public string? EvidenceType { get; init; }

    public string? OriginalFileName { get; init; }

    public string? FileType { get; init; }

    public long? FileSize { get; init; }

    public DateOnly? DocumentDate { get; init; }

    public string? ProviderName { get; init; }

    public string? UserNotes { get; init; }
}
