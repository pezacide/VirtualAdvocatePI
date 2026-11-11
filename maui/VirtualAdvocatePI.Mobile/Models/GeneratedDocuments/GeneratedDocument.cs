namespace VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;

public sealed class GeneratedDocument
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public string DocumentType { get; init; } = string.Empty;

    public string DocumentStatus { get; init; } = string.Empty;

    public string? DocxStoragePath { get; init; }

    public string? PdfStoragePath { get; init; }

    public string TemplateVersion { get; init; } = string.Empty;

    public string? IncludedAiDraftIds { get; init; }

    public DateTimeOffset? GeneratedAt { get; init; }

    public DateTimeOffset? DownloadedAt { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public string DocumentTypeLabel => DocumentType switch
    {
        "CLAIM_STARTER_PACK" => "Claim Starter Pack",
        "DOCTOR_GUIDANCE_PACK" => "Doctor Guidance Pack",
        _ => DocumentType
    };

    public string DocumentStatusLabel => DocumentStatus switch
    {
        "GENERATED" => "Generated",
        "DOWNLOADED" => "Downloaded",
        "DRAFT" => "Draft",
        _ => DocumentStatus
    };

    public bool HasDocx => !string.IsNullOrWhiteSpace(DocxStoragePath);

    public bool HasPdf => !string.IsNullOrWhiteSpace(PdfStoragePath);

    public string GeneratedAtText => GeneratedAt is { } value
        ? value.LocalDateTime.ToString("dd MMM yyyy, h:mm tt")
        : "Not generated yet";
}
