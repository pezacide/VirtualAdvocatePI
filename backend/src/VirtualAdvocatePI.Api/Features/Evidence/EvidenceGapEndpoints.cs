using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Users;

namespace VirtualAdvocatePI.Api.Features.Evidence;

public static class EvidenceGapEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceGapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-gaps", async (
            Guid workspaceId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await UserOwnsWorkspaceAsync(db, user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var gaps = await db.EvidenceGaps
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.Severity)
                .ThenBy(x => x.GapType)
                .ToListAsync();

            return Results.Ok(gaps.Select(ToEvidenceGapResponse).ToList());
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-gaps", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await UserOwnsConditionAsync(db, user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var gaps = await db.EvidenceGaps
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.Severity)
                .ThenBy(x => x.GapType)
                .ToListAsync();

            return Results.Ok(gaps.Select(ToEvidenceGapResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-gaps/recalculate", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await UserOwnsConditionAsync(db, user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var condition = await db.ClaimConditions
                .FirstOrDefaultAsync(x =>
                    x.Id == conditionId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (condition is null)
            {
                return Results.NotFound();
            }

            var evidenceItems = await db.EvidenceItems
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED" &&
                    x.EvidenceStatus != "MISSING" &&
                    x.EvidenceStatus != "NOT_APPLICABLE")
                .ToListAsync();

            var acceptedHistory = await db.AcceptedConditionHistories
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED")
                .ToListAsync();

            await ArchiveExistingOpenGapsAsync(db, workspaceId, conditionId);

            var createdGaps = new List<EvidenceGap>();

            if (!HasAnyEvidenceType(evidenceItems, "MEDICAL_REPORT", "SPECIALIST_REPORT"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "DIAGNOSIS_EVIDENCE_MISSING",
                    "HIGH",
                    "No medical report or specialist report has been listed or uploaded for this condition. Diagnosis evidence may be useful for review by a doctor, advocate, lawyer, support person or DVA.",
                    "Consider listing or uploading a medical report, specialist report, or other document that confirms the diagnosis or current clinical picture."));
            }

            if (!HasAnyEvidenceType(evidenceItems, "TREATMENT_SUMMARY", "MEDICAL_REPORT", "SPECIALIST_REPORT"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "CURRENT_TREATMENT_EVIDENCE_MISSING",
                    "MEDIUM",
                    "No treatment summary, medical report or specialist report has been listed or uploaded for this condition. Current treatment information may help explain the present state of the condition.",
                    "Consider gathering treatment summaries, GP notes, specialist letters or other records showing current treatment."));
            }

            if (!string.IsNullOrWhiteSpace(condition.MedicationSummary) &&
                !HasAnyEvidenceType(evidenceItems, "MEDICATION_LIST", "TREATMENT_SUMMARY", "MEDICAL_REPORT"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "MEDICATION_EVIDENCE_MISSING",
                    "LOW",
                    "Medication has been mentioned for this condition, but no medication list or related treatment evidence has been listed or uploaded.",
                    "Consider adding a medication list, pharmacy record, GP summary or treatment summary if medication is relevant to the claim preparation pack."));
            }

            if (string.IsNullOrWhiteSpace(condition.FunctionalImpactSummary) &&
                !HasAnyEvidenceType(evidenceItems, "FUNCTIONAL_IMPACT_NOTES", "PERSONAL_STATEMENT"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "FUNCTIONAL_IMPACT_NOTES_MISSING",
                    "MEDIUM",
                    "No functional impact notes or personal statement have been recorded for this condition. Functional impact information may help explain how the condition affects daily life.",
                    "Consider adding plain-English notes about daily activities, work impact, social impact, domestic tasks, mobility, self-care, flare-ups or restrictions."));
            }

            var acceptedOrCompensated = acceptedHistory.Any(x =>
                x.PreviouslyAcceptedByDva == "YES" ||
                x.PreviousCompensationReceived == "YES");

            if (acceptedOrCompensated && !HasAnyEvidenceType(evidenceItems, "DVA_DECISION_LETTER"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "PREVIOUS_DVA_DECISION_LETTER_MISSING",
                    "HIGH",
                    "This condition has been marked as previously accepted or compensated, but no DVA decision letter has been listed or uploaded.",
                    "Consider adding the previous DVA decision letter if available, or noting that it needs to be requested or located."));
            }

            if (acceptedOrCompensated && !HasAnyEvidenceType(evidenceItems, "PREVIOUS_PI_ASSESSMENT", "DCP_ASSESSMENT"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "PREVIOUS_ASSESSMENT_LETTER_MISSING",
                    "HIGH",
                    "This condition has previous acceptance or compensation history, but no previous PI or DCP assessment evidence has been listed or uploaded.",
                    "Consider adding previous PI, DCP or assessment material if available. This may be useful context for a worsening or post-reform review."));
            }

            var worseningMentioned =
                !string.IsNullOrWhiteSpace(condition.WorseningNotes) ||
                acceptedHistory.Any(x => x.WorseningClaimed == "YES");

            if (worseningMentioned && !HasAnyEvidenceType(evidenceItems, "MEDICAL_REPORT", "SPECIALIST_REPORT", "TREATMENT_SUMMARY"))
            {
                createdGaps.Add(CreateGap(
                    workspaceId,
                    conditionId,
                    "WORSENING_EVIDENCE_MISSING",
                    "HIGH",
                    "Worsening has been mentioned, but no medical report, specialist report or treatment summary has been listed or uploaded to support the change over time.",
                    "Consider gathering medical evidence that describes what has changed, when it changed, current severity, treatment changes and functional impact."));
            }

            if (createdGaps.Count > 0)
            {
                db.EvidenceGaps.AddRange(createdGaps);
            }

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_GAPS_RECALCULATED",
                $"Evidence gaps recalculated. ConditionId={conditionId}; Created={createdGaps.Count}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                conditionId,
                createdCount = createdGaps.Count,
                gaps = createdGaps.Select(ToEvidenceGapResponse).ToList()
            });
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-gaps/{gapId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid gapId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db,
            UpdateEvidenceGapRequest input) =>
        {
            var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await UserOwnsConditionAsync(db, user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var gap = await db.EvidenceGaps
                .FirstOrDefaultAsync(x =>
                    x.Id == gapId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (gap is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.GapStatus))
            {
                var gapStatus = NormaliseGapStatus(input.GapStatus);

                if (!IsValidGapStatus(gapStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid gap status.",
                        allowedValues = GetAllowedGapStatuses()
                    });
                }

                gap.GapStatus = gapStatus;
            }

            if (!string.IsNullOrWhiteSpace(input.Severity))
            {
                var severity = NormaliseSeverity(input.Severity);

                if (!IsValidSeverity(severity))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid severity.",
                        allowedValues = GetAllowedSeverities()
                    });
                }

                gap.Severity = severity;
            }

            if (!string.IsNullOrWhiteSpace(input.PlainEnglishExplanation))
            {
                gap.PlainEnglishExplanation = input.PlainEnglishExplanation.Trim();
            }

            if (input.SuggestedNextStep is not null)
            {
                gap.SuggestedNextStep = input.SuggestedNextStep;
            }

            gap.UpdatedAt = DateTimeOffset.UtcNow;

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_GAP_UPDATED",
                $"Evidence gap updated. GapId={gap.Id}; GapType={gap.GapType}");

            await db.SaveChangesAsync();

            return Results.Ok(ToEvidenceGapResponse(gap));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-gaps/{gapId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid gapId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await UserOwnsConditionAsync(db, user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var gap = await db.EvidenceGaps
                .FirstOrDefaultAsync(x =>
                    x.Id == gapId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (gap is null)
            {
                return Results.NotFound();
            }

            gap.Status = "ARCHIVED";
            gap.UpdatedAt = DateTimeOffset.UtcNow;

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_GAP_ARCHIVED",
                $"Evidence gap archived. GapId={gap.Id}; GapType={gap.GapType}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = gap.Id,
                status = gap.Status,
                archived = true
            });
        });

        return app;
    }

    private static async Task<AppUser?> GetOrCreateCurrentUserAsync(
        HttpRequest request,
        FirebaseAuthService firebaseAuthService,
        VirtualAdvocateDbContext db)
    {
        AuthenticatedFirebaseUser? firebaseUser;

        try
        {
            firebaseUser = await firebaseAuthService.VerifyBearerTokenAsync(request);
        }
        catch
        {
            return null;
        }

        if (firebaseUser is null)
        {
            return null;
        }

        var email = firebaseUser.Email ?? string.Empty;

        var user = await db.Users.FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUser.FirebaseUid);

        if (user is null)
        {
            user = new AppUser
            {
                FirebaseUid = firebaseUser.FirebaseUid,
                Email = email,
                DisplayName = firebaseUser.DisplayName,
                Role = "VETERAN",
                AccountStatus = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow
            };

            db.Users.Add(user);
        }
        else
        {
            user.Email = email;
            user.DisplayName = firebaseUser.DisplayName;
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.AccountStatus = "ACTIVE";
        }

        await db.SaveChangesAsync();

        return user;
    }

    private static async Task<bool> UserOwnsWorkspaceAsync(VirtualAdvocateDbContext db, Guid userId, Guid workspaceId)
    {
        return await db.ClaimWorkspaces.AnyAsync(x =>
            x.Id == workspaceId &&
            x.UserId == userId &&
            x.Status != "ARCHIVED");
    }

    private static async Task<bool> UserOwnsConditionAsync(VirtualAdvocateDbContext db, Guid userId, Guid workspaceId, Guid conditionId)
    {
        return await db.ClaimWorkspaces.AnyAsync(x =>
                x.Id == workspaceId &&
                x.UserId == userId &&
                x.Status != "ARCHIVED")
            && await db.ClaimConditions.AnyAsync(x =>
                x.Id == conditionId &&
                x.ClaimWorkspaceId == workspaceId &&
                x.Status != "ARCHIVED");
    }

    private static async Task ArchiveExistingOpenGapsAsync(
        VirtualAdvocateDbContext db,
        Guid workspaceId,
        Guid conditionId)
    {
        var existingGaps = await db.EvidenceGaps
            .Where(x =>
                x.ClaimWorkspaceId == workspaceId &&
                x.ConditionId == conditionId &&
                x.Status != "ARCHIVED")
            .ToListAsync();

        foreach (var gap in existingGaps)
        {
            gap.Status = "ARCHIVED";
            gap.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private static bool HasAnyEvidenceType(List<EvidenceItem> evidenceItems, params string[] evidenceTypes)
    {
        return evidenceItems.Any(x => evidenceTypes.Contains(x.EvidenceType));
    }

    private static EvidenceGap CreateGap(
        Guid workspaceId,
        Guid conditionId,
        string gapType,
        string severity,
        string explanation,
        string suggestedNextStep)
    {
        return new EvidenceGap
        {
            ClaimWorkspaceId = workspaceId,
            ConditionId = conditionId,
            GapType = gapType,
            GapStatus = "OPEN",
            Severity = severity,
            PlainEnglishExplanation = explanation,
            SuggestedNextStep = suggestedNextStep,
            Status = "ACTIVE",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static void AddAuditEvent(
        VirtualAdvocateDbContext db,
        HttpRequest request,
        Guid userId,
        Guid workspaceId,
        string eventType,
        string? eventDetail)
    {
        request.Headers.TryGetValue("User-Agent", out var userAgent);

        db.AuditEvents.Add(new AuditEvent
        {
            UserId = userId,
            ClaimWorkspaceId = workspaceId,
            EventType = eventType,
            EventDetail = eventDetail,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            ClientType = userAgent.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static object ToEvidenceGapResponse(EvidenceGap gap)
    {
        return new
        {
            id = gap.Id,
            claimWorkspaceId = gap.ClaimWorkspaceId,
            conditionId = gap.ConditionId,
            gapType = gap.GapType,
            gapStatus = gap.GapStatus,
            severity = gap.Severity,
            plainEnglishExplanation = gap.PlainEnglishExplanation,
            suggestedNextStep = gap.SuggestedNextStep,
            status = gap.Status,
            createdAt = gap.CreatedAt,
            updatedAt = gap.UpdatedAt
        };
    }

    private static string NormaliseGapStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "OPEN" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseSeverity(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "MEDIUM" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidGapStatus(string value)
    {
        return GetAllowedGapStatuses().Contains(value);
    }

    private static bool IsValidSeverity(string value)
    {
        return GetAllowedSeverities().Contains(value);
    }

    private static string[] GetAllowedGapStatuses()
    {
        return new[]
        {
            "OPEN",
            "IN_PROGRESS",
            "RESOLVED",
            "USER_MARKED_NOT_APPLICABLE"
        };
    }

    private static string[] GetAllowedSeverities()
    {
        return new[]
        {
            "LOW",
            "MEDIUM",
            "HIGH"
        };
    }
}

public sealed record UpdateEvidenceGapRequest(
    string? GapStatus,
    string? Severity,
    string? PlainEnglishExplanation,
    string? SuggestedNextStep
);
