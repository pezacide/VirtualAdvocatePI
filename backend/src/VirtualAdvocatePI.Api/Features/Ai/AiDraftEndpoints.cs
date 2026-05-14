using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Ai;

public static class AiDraftEndpoints
{
    public static IEndpointRouteBuilder MapAiDraftEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var drafts = await db.AiDrafts
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(drafts.Select(ToAiDraftResponse).ToList());
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/ai-drafts", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var drafts = await db.AiDrafts
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(drafts.Select(ToAiDraftResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateAiDraftRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            if (input.ConditionId.HasValue &&
                !await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, input.ConditionId.Value))
            {
                return Results.NotFound();
            }

            var draftType = NormaliseDraftType(input.DraftType);

            if (!IsValidDraftType(draftType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid AI draft type.",
                    allowedValues = GetAllowedDraftTypes()
                });
            }

            if (string.IsNullOrWhiteSpace(input.DraftText))
            {
                return Results.BadRequest(new { error = "Draft text is required." });
            }

            var reviewStatus = NormaliseReviewStatus(input.ReviewStatus);

            if (!IsValidReviewStatus(reviewStatus))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid review status.",
                    allowedValues = GetAllowedReviewStatuses()
                });
            }

            var draft = new AiDraft
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = input.ConditionId,
                DraftType = draftType,
                PromptVersion = string.IsNullOrWhiteSpace(input.PromptVersion)
                    ? "manual-metadata-v1"
                    : input.PromptVersion.Trim(),
                SourceReferences = input.SourceReferences,
                DraftText = input.DraftText.Trim(),
                UserEditedText = input.UserEditedText,
                ReviewStatus = reviewStatus,
                ApprovedAt = reviewStatus == "APPROVED" ? DateTimeOffset.UtcNow : null,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.AiDrafts.Add(draft);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_DRAFT_CREATED",
                $"AI draft metadata created. DraftType={draftType}; DraftId={draft.Id}");

            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/claim-workspaces/{workspaceId}/ai-drafts/{draft.Id}", ToAiDraftResponse(draft));
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts/{draftId:guid}", async (
            Guid workspaceId,
            Guid draftId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var draft = await db.AiDrafts
                .FirstOrDefaultAsync(x =>
                    x.Id == draftId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (draft is null)
            {
                return Results.NotFound();
            }

            var previousReviewStatus = draft.ReviewStatus;

            return Results.Ok(ToAiDraftResponse(draft));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts/{draftId:guid}", async (
            Guid workspaceId,
            Guid draftId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateAiDraftRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var draft = await db.AiDrafts
                .FirstOrDefaultAsync(x =>
                    x.Id == draftId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (draft is null)
            {
                return Results.NotFound();
            }

            var previousReviewStatus = draft.ReviewStatus;

            if (!string.IsNullOrWhiteSpace(input.DraftType))
            {
                var draftType = NormaliseDraftType(input.DraftType);

                if (!IsValidDraftType(draftType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid AI draft type.",
                        allowedValues = GetAllowedDraftTypes()
                    });
                }

                draft.DraftType = draftType;
            }

            if (!string.IsNullOrWhiteSpace(input.PromptVersion))
            {
                draft.PromptVersion = input.PromptVersion.Trim();
            }

            if (input.SourceReferences is not null)
            {
                draft.SourceReferences = input.SourceReferences;
            }

            if (!string.IsNullOrWhiteSpace(input.DraftText))
            {
                draft.DraftText = input.DraftText.Trim();
            }

            if (input.UserEditedText is not null)
            {
                draft.UserEditedText = input.UserEditedText;
            }

            if (!string.IsNullOrWhiteSpace(input.ReviewStatus))
            {
                var reviewStatus = NormaliseReviewStatus(input.ReviewStatus);

                if (!IsValidReviewStatus(reviewStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid review status.",
                        allowedValues = GetAllowedReviewStatuses()
                    });
                }

                draft.ReviewStatus = reviewStatus;

                if (reviewStatus == "APPROVED")
                {
                    draft.ApprovedAt ??= DateTimeOffset.UtcNow;
                }

                if (reviewStatus is "REJECTED" or "USER_REVIEW_REQUIRED" or "REGENERATED")
                {
                    draft.ApprovedAt = null;
                }
            }

            draft.UpdatedAt = DateTimeOffset.UtcNow;

            if (!string.Equals(previousReviewStatus, draft.ReviewStatus, StringComparison.OrdinalIgnoreCase))
            {
                var statusEventType = draft.ReviewStatus switch
                {
                    "APPROVED" => "AI_DRAFT_APPROVED",
                    "REJECTED" => "AI_DRAFT_REJECTED",
                    "REGENERATED" => "AI_DRAFT_REGENERATED",
                    "USER_EDITED" => "AI_DRAFT_USER_EDITED",
                    "USER_REVIEW_REQUIRED" => "AI_DRAFT_REVIEW_REQUIRED",
                    _ => "AI_DRAFT_REVIEW_STATUS_CHANGED"
                };

                auditService.AddAuditEvent(
                    request,
                    user.Id,
                    workspaceId,
                    statusEventType,
                    $"AI draft review status changed. DraftType={draft.DraftType}; PreviousReviewStatus={previousReviewStatus}; ReviewStatus={draft.ReviewStatus}; DraftId={draft.Id}");
            }

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_DRAFT_UPDATED",
                $"AI draft metadata updated. DraftType={draft.DraftType}; ReviewStatus={draft.ReviewStatus}; DraftId={draft.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToAiDraftResponse(draft));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts/{draftId:guid}", async (
            Guid workspaceId,
            Guid draftId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var draft = await db.AiDrafts
                .FirstOrDefaultAsync(x =>
                    x.Id == draftId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (draft is null)
            {
                return Results.NotFound();
            }

            var previousReviewStatus = draft.ReviewStatus;

            draft.Status = "ARCHIVED";
            draft.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_DRAFT_ARCHIVED",
                $"AI draft archived. DraftType={draft.DraftType}; DraftId={draft.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = draft.Id,
                status = draft.Status,
                archived = true
            });
        });

        return app;
    }

    private static object ToAiDraftResponse(AiDraft draft)
    {
        return new
        {
            id = draft.Id,
            claimWorkspaceId = draft.ClaimWorkspaceId,
            conditionId = draft.ConditionId,
            draftType = draft.DraftType,
            promptVersion = draft.PromptVersion,
            sourceReferences = draft.SourceReferences,
            draftText = draft.DraftText,
            userEditedText = draft.UserEditedText,
            reviewStatus = draft.ReviewStatus,
            approvedAt = draft.ApprovedAt,
            status = draft.Status,
            createdAt = draft.CreatedAt,
            updatedAt = draft.UpdatedAt
        };
    }

    private static string NormaliseDraftType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "VETERAN_STATEMENT" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseReviewStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "USER_REVIEW_REQUIRED" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidDraftType(string value)
    {
        return GetAllowedDraftTypes().Contains(value);
    }

    private static bool IsValidReviewStatus(string value)
    {
        return GetAllowedReviewStatuses().Contains(value);
    }

    private static string[] GetAllowedDraftTypes()
    {
        return new[]
        {
            "VETERAN_STATEMENT",
            "WORSENING_SUMMARY",
            "EVIDENCE_GAP_SUMMARY",
            "DOCTOR_APPOINTMENT_QUESTIONS",
            "DOCTOR_REQUEST_LETTER",
            "CLAIM_PACK_COVER_NOTE"
        };
    }

    private static string[] GetAllowedReviewStatuses()
    {
        return new[]
        {
            "DRAFT_CREATED",
            "USER_REVIEW_REQUIRED",
            "USER_EDITED",
            "APPROVED",
            "REJECTED",
            "REGENERATED"
        };
    }
}

public sealed record CreateAiDraftRequest(
    Guid? ConditionId,
    string? DraftType,
    string? PromptVersion,
    string? SourceReferences,
    string? DraftText,
    string? UserEditedText,
    string? ReviewStatus
);

public sealed record UpdateAiDraftRequest(
    string? DraftType,
    string? PromptVersion,
    string? SourceReferences,
    string? DraftText,
    string? UserEditedText,
    string? ReviewStatus
);
