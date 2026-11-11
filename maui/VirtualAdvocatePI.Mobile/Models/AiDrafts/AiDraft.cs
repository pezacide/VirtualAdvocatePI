namespace VirtualAdvocatePI.Mobile.Models.AiDrafts;

public sealed class AiDraft
{
    public Guid Id { get; init; }

    public Guid ClaimWorkspaceId { get; init; }

    public Guid? ConditionId { get; init; }

    public string DraftType { get; init; } = string.Empty;

    public string PromptVersion { get; init; } = string.Empty;

    public string? SourceReferences { get; init; }

    public string DraftText { get; init; } = string.Empty;

    public string? UserEditedText { get; init; }

    public string ReviewStatus { get; init; } = string.Empty;

    public DateTimeOffset? ApprovedAt { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public string DraftTypeLabel => DraftType switch
    {
        "VETERAN_STATEMENT" => "Veteran statement",
        "WORSENING_SUMMARY" => "Worsening summary",
        "EVIDENCE_GAP_SUMMARY" => "Evidence gap summary",
        "DOCTOR_APPOINTMENT_QUESTIONS" => "Doctor appointment questions",
        "DOCTOR_REQUEST_LETTER" => "Doctor request letter",
        "CLAIM_PACK_COVER_NOTE" => "Claim pack cover note",
        _ => DraftType
    };

    public string ReviewStatusLabel => ReviewStatus switch
    {
        "DRAFT_CREATED" => "Draft created",
        "USER_REVIEW_REQUIRED" => "Needs review",
        "USER_EDITED" => "Edited by you",
        "APPROVED" => "Approved",
        "REJECTED" => "Rejected",
        "REGENERATED" => "Regenerated",
        _ => ReviewStatus
    };

    public bool IsApproved => string.Equals(ReviewStatus, "APPROVED", StringComparison.OrdinalIgnoreCase);

    public bool HasSourceReferences => !string.IsNullOrWhiteSpace(SourceReferences);

    public string SourceReferencesDisplay => string.IsNullOrWhiteSpace(SourceReferences)
        ? "No source references recorded for this draft."
        : SourceReferences!;

    public string UpdatedAtText => UpdatedAt == default
        ? string.Empty
        : UpdatedAt.LocalDateTime.ToString("dd MMM yyyy, h:mm tt");
}
