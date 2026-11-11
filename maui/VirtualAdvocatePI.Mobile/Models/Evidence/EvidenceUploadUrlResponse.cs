namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceUploadUrlResponse
{
    public EvidenceItem? EvidenceItem { get; init; }

    public EvidenceUploadInstruction? Upload { get; init; }
}

public sealed class EvidenceUploadInstruction
{
    public string Method { get; init; } = "PUT";

    public string Url { get; init; } = string.Empty;

    public int ExpiresInMinutes { get; init; }

    public string? Note { get; init; }
}
