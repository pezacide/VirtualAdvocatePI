using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class AcceptedConditionHistoryEndpoints
{
    public static IEndpointRouteBuilder MapAcceptedConditionHistoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history", async (
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

            var rows = await db.AcceptedConditionHistories
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.ConditionId == conditionId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(rows.Select(ToAcceptedHistoryResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateAcceptedHistoryRequest input) =>
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

            var originalAct = NormaliseOriginalAct(input.OriginalAct);

            if (!IsValidOriginalAct(originalAct))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid original Act.",
                    allowedValues = GetAllowedOriginalActs()
                });
            }

            var history = new AcceptedConditionHistory
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = conditionId,
                PreviouslyAcceptedByDva = NormaliseYesNoUnsure(input.PreviouslyAcceptedByDva),
                OriginalAct = originalAct,
                PreviousCompensationReceived = NormaliseYesNoUnsure(input.PreviousCompensationReceived),
                PreviousDvaDecisionLetterAvailable = NormaliseYesNoUnsure(input.PreviousDvaDecisionLetterAvailable),
                PreviousAssessmentLetterAvailable = NormaliseYesNoUnsure(input.PreviousAssessmentLetterAvailable),
                PreviousDecisionDate = input.PreviousDecisionDate,
                PreviousAssessmentDate = input.PreviousAssessmentDate,
                WorseningClaimed = NormaliseYesNoUnsure(input.WorseningClaimed),
                WorseningSummary = input.WorseningSummary,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.AcceptedConditionHistories.Add(history);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "ACCEPTED_CONDITION_HISTORY_CREATED",
                $"Accepted-condition history created. ConditionId={conditionId}; HistoryId={history.Id}; OriginalAct={originalAct}");

            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{history.Id}", ToAcceptedHistoryResponse(history));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history/{historyId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid historyId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateAcceptedHistoryRequest input) =>
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

            var history = await db.AcceptedConditionHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (history is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.PreviouslyAcceptedByDva))
            {
                history.PreviouslyAcceptedByDva = NormaliseYesNoUnsure(input.PreviouslyAcceptedByDva);
            }

            if (!string.IsNullOrWhiteSpace(input.OriginalAct))
            {
                var originalAct = NormaliseOriginalAct(input.OriginalAct);

                if (!IsValidOriginalAct(originalAct))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid original Act.",
                        allowedValues = GetAllowedOriginalActs()
                    });
                }

                history.OriginalAct = originalAct;
            }

            if (!string.IsNullOrWhiteSpace(input.PreviousCompensationReceived))
            {
                history.PreviousCompensationReceived = NormaliseYesNoUnsure(input.PreviousCompensationReceived);
            }

            if (!string.IsNullOrWhiteSpace(input.PreviousDvaDecisionLetterAvailable))
            {
                history.PreviousDvaDecisionLetterAvailable = NormaliseYesNoUnsure(input.PreviousDvaDecisionLetterAvailable);
            }

            if (!string.IsNullOrWhiteSpace(input.PreviousAssessmentLetterAvailable))
            {
                history.PreviousAssessmentLetterAvailable = NormaliseYesNoUnsure(input.PreviousAssessmentLetterAvailable);
            }

            if (!string.IsNullOrWhiteSpace(input.WorseningClaimed))
            {
                history.WorseningClaimed = NormaliseYesNoUnsure(input.WorseningClaimed);
            }

            history.PreviousDecisionDate = input.PreviousDecisionDate ?? history.PreviousDecisionDate;
            history.PreviousAssessmentDate = input.PreviousAssessmentDate ?? history.PreviousAssessmentDate;
            history.WorseningSummary = input.WorseningSummary ?? history.WorseningSummary;
            history.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "ACCEPTED_CONDITION_HISTORY_UPDATED",
                $"Accepted-condition history updated. ConditionId={conditionId}; HistoryId={history.Id}; OriginalAct={history.OriginalAct}");

            await db.SaveChangesAsync();

            return Results.Ok(ToAcceptedHistoryResponse(history));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history/{historyId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid historyId,
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

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var history = await db.AcceptedConditionHistories
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (history is null)
            {
                return Results.NotFound();
            }

            history.Status = "ARCHIVED";
            history.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "ACCEPTED_CONDITION_HISTORY_ARCHIVED",
                $"Accepted-condition history archived. ConditionId={conditionId}; HistoryId={history.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = history.Id,
                status = history.Status,
                archived = true
            });
        });

        return app;
    }

    internal static object ToAcceptedHistoryResponse(AcceptedConditionHistory history)
    {
        return new
        {
            id = history.Id,
            claimWorkspaceId = history.ClaimWorkspaceId,
            conditionId = history.ConditionId,
            previouslyAcceptedByDva = history.PreviouslyAcceptedByDva,
            originalAct = history.OriginalAct,
            previousCompensationReceived = history.PreviousCompensationReceived,
            previousDvaDecisionLetterAvailable = history.PreviousDvaDecisionLetterAvailable,
            previousAssessmentLetterAvailable = history.PreviousAssessmentLetterAvailable,
            previousDecisionDate = history.PreviousDecisionDate,
            previousAssessmentDate = history.PreviousAssessmentDate,
            worseningClaimed = history.WorseningClaimed,
            worseningSummary = history.WorseningSummary,
            status = history.Status,
            createdAt = history.CreatedAt,
            updatedAt = history.UpdatedAt
        };
    }

    private static string NormaliseOriginalAct(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "UNKNOWN" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseYesNoUnsure(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "UNSURE";
        }

        var normalised = value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "YES" => "YES",
            "NO" => "NO",
            "UNSURE" => "UNSURE",
            "NOT_APPLICABLE" => "NOT_APPLICABLE",
            "N/A" => "NOT_APPLICABLE",
            _ => "UNSURE"
        };
    }

    private static bool IsValidOriginalAct(string value) => GetAllowedOriginalActs().Contains(value);

    private static string[] GetAllowedOriginalActs()
    {
        return new[]
        {
            "VEA",
            "DRCA",
            "MRCA",
            "UNKNOWN",
            "NOT_APPLICABLE"
        };
    }
}

public sealed record CreateAcceptedHistoryRequest(
    string? PreviouslyAcceptedByDva,
    string? OriginalAct,
    string? PreviousCompensationReceived,
    string? PreviousDvaDecisionLetterAvailable,
    string? PreviousAssessmentLetterAvailable,
    DateOnly? PreviousDecisionDate,
    DateOnly? PreviousAssessmentDate,
    string? WorseningClaimed,
    string? WorseningSummary
);

public sealed record UpdateAcceptedHistoryRequest(
    string? PreviouslyAcceptedByDva,
    string? OriginalAct,
    string? PreviousCompensationReceived,
    string? PreviousDvaDecisionLetterAvailable,
    string? PreviousAssessmentLetterAvailable,
    DateOnly? PreviousDecisionDate,
    DateOnly? PreviousAssessmentDate,
    string? WorseningClaimed,
    string? WorseningSummary
);
