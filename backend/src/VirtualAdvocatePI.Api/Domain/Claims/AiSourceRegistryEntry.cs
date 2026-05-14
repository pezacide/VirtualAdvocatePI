namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class AiSourceRegistryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string SourceKey { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string Jurisdiction { get; set; } = string.Empty;

    public string? SourceVersion { get; set; }

    public DateOnly? SourceDate { get; set; }

    public string CitationLabel { get; set; } = string.Empty;

    public string? SourceUrl { get; set; }

    public string? StoragePath { get; set; }

    public string? ContentHash { get; set; }

    public string ApprovalStatus { get; set; } = "DRAFT";

    public bool IsActive { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ReviewNotes { get; set; }

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}