namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceDownloadUrlResponse
{
    public Guid EvidenceItemId { get; init; }

    public string Method { get; init; } = "GET";

    public string Url { get; init; } = string.Empty;

    public int ExpiresInMinutes { get; init; }
}
