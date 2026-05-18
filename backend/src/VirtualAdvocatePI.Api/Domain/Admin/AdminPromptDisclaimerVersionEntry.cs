namespace VirtualAdvocatePI.Api.Domain.Admin;

public sealed class AdminPromptDisclaimerVersionEntry
{
    public Guid Id { get; set; }

    public string VersionKey { get; set; } = string.Empty;

    public string VersionType { get; set; } = "PROMPT";

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "GENERAL";

    public string VersionLabel { get; set; } = "v1";

    public string AppliesTo { get; set; } = "GENERAL";

    public string Content { get; set; } = string.Empty;

    public string ApprovalStatus { get; set; } = "DRAFT";

    public string? ApprovedBy { get; set; }

    public string? ReviewNotes { get; set; }

    public DateTimeOffset? EffectiveFrom { get; set; }

    public DateTimeOffset? RetiredAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}