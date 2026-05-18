namespace VirtualAdvocatePI.Api.Domain.Admin;

public sealed class AdminTemplateRegistryEntry
{
    public Guid Id { get; set; }

    public string TemplateKey { get; set; } = string.Empty;

    public string TemplateType { get; set; } = "QUESTION";

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = "GENERAL";

    public string TemplateVersion { get; set; } = "v1";

    public string TemplateBody { get; set; } = string.Empty;

    public string OutputFormat { get; set; } = "TEXT";

    public string ApprovalStatus { get; set; } = "DRAFT";

    public string? ApprovedBy { get; set; }

    public string? ReviewNotes { get; set; }

    public bool IsActive { get; set; } = true;

    public string Status { get; set; } = "ACTIVE";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}