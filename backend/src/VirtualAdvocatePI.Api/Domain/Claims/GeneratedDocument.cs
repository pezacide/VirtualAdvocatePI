namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class GeneratedDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public string DocumentType { get; set; } = "POST_2026_PI_CLAIM_STARTER_PACK";

    public string DocumentStatus { get; set; } = "REQUESTED";

    public string? DocxStoragePath { get; set; }

    public string? PdfStoragePath { get; set; }

    public string TemplateVersion { get; set; } = "template-v1";

    public string? IncludedAiDraftIds { get; set; }

    public DateTimeOffset? GeneratedAt { get; set; }

    public DateTimeOffset? DownloadedAt { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
