namespace VirtualAdvocatePI.Api.Domain.Claims;

public sealed class AiDraft
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClaimWorkspaceId { get; set; }

    public Guid? ConditionId { get; set; }

    public string DraftType { get; set; } = "VETERAN_STATEMENT";

    public string PromptVersion { get; set; } = "manual-metadata-v1";

    public string? SourceReferences { get; set; }

    public string DraftText { get; set; } = string.Empty;

    public string? UserEditedText { get; set; }

    public string ReviewStatus { get; set; } = "USER_REVIEW_REQUIRED";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ApprovedAt { get; set; }

    public string Status { get; set; } = "ACTIVE";
}
