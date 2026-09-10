namespace VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;

public sealed class GeneratedDocumentDownloadUrlResponse
{
    public Guid DocumentId { get; init; }

    public string Format { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public string Method { get; init; } = "GET";

    public int ExpiresInMinutes { get; init; }
}
