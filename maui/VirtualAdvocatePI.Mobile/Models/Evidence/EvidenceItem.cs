namespace VirtualAdvocatePI.Mobile.Models.Evidence;

public sealed class EvidenceItem
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public Guid? ConditionId { get; init; }

    public string EvidenceType { get; init; } = string.Empty;

    public string EvidenceStatus { get; init; } = string.Empty;

    public string? OriginalFileName { get; init; }

    public string? StoragePath { get; init; }

    public string? FileType { get; init; }

    public long? FileSize { get; init; }

    public DateOnly? DocumentDate { get; init; }

    public string? ProviderName { get; init; }

    public string? UserNotes { get; init; }

    public string? AiSummary { get; init; }

    public string? UserConfirmedSummary { get; init; }

    public bool UsedInGeneratedPack { get; init; }

    public DateTimeOffset? UploadedAt { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public bool IsUploaded => string.Equals(EvidenceStatus, "UPLOADED", StringComparison.OrdinalIgnoreCase)
        || UploadedAt is not null;

    public string EvidenceTypeLabel => EvidenceTypeOption.LabelFor(EvidenceType);

    public string EvidenceStatusLabel => EvidenceStatus switch
    {
        "MISSING" => "Missing",
        "LISTED_NOT_UPLOADED" => "Listed, not uploaded",
        "UPLOADED" => "Uploaded",
        "REVIEWED" => "Reviewed",
        "CONFIRMED" => "Confirmed",
        "NOT_APPLICABLE" => "Not applicable",
        _ => EvidenceStatus
    };

    public string DisplayFileName => string.IsNullOrWhiteSpace(OriginalFileName)
        ? "No file attached yet"
        : OriginalFileName!;

    public string ProviderAndDateLine
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(ProviderName))
            {
                parts.Add(ProviderName!);
            }

            if (DocumentDate is { } date)
            {
                parts.Add(date.ToString("dd MMM yyyy"));
            }

            return parts.Count == 0 ? string.Empty : string.Join(" • ", parts);
        }
    }

    public bool HasProviderOrDate => !string.IsNullOrWhiteSpace(ProviderAndDateLine);

    public bool HasNotes => !string.IsNullOrWhiteSpace(UserNotes);
}
