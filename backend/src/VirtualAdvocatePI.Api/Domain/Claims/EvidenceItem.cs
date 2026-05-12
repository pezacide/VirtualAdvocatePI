namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class EvidenceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid? ConditionId { get; set; }

    public string EvidenceType { get; set; } = "OTHER";

    public string EvidenceStatus { get; set; } = "LISTED_NOT_UPLOADED";

    public string? OriginalFileName { get; set; }

    public string? StoragePath { get; set; }

    public string? FileType { get; set; }

    public long? FileSize { get; set; }

    public DateOnly? DocumentDate { get; set; }

    public string? ProviderName { get; set; }

    public string? UserNotes { get; set; }

    public string? AiSummary { get; set; }

    public string? UserConfirmedSummary { get; set; }

    public bool UsedInGeneratedPack { get; set; }

    public DateTimeOffset? UploadedAt { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
