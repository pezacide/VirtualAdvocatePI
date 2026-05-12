using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Users;

namespace VirtualAdvocatePI.Api.Features.Evidence;

public static class EvidenceAndAuditEndpoints
{
    public static IEndpointRouteBuilder MapEvidenceAndAuditEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items", async (
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

            var rows = await db.EvidenceItems
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(rows.Select(ToEvidenceItemResponse).ToList());
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-items", async (
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

            var rows = await db.EvidenceItems
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(rows.Select(ToEvidenceItemResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-items", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db,
            CreateEvidenceItemRequest input) =>
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

            var evidenceType = NormaliseEvidenceType(input.EvidenceType);

            if (!IsValidEvidenceType(evidenceType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid evidence type.",
                    allowedValues = GetAllowedEvidenceTypes()
                });
            }

            var evidenceStatus = NormaliseEvidenceStatus(input.EvidenceStatus);

            if (!IsValidEvidenceStatus(evidenceStatus))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid evidence status.",
                    allowedValues = GetAllowedEvidenceStatuses()
                });
            }

            var evidenceItem = new EvidenceItem
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = conditionId,
                EvidenceType = evidenceType,
                EvidenceStatus = evidenceStatus,
                OriginalFileName = input.OriginalFileName,
                StoragePath = input.StoragePath,
                FileType = input.FileType,
                FileSize = input.FileSize,
                DocumentDate = input.DocumentDate,
                ProviderName = input.ProviderName,
                UserNotes = input.UserNotes,
                AiSummary = input.AiSummary,
                UserConfirmedSummary = input.UserConfirmedSummary,
                UsedInGeneratedPack = input.UsedInGeneratedPack ?? false,
                UploadedAt = evidenceStatus == "UPLOADED" ? DateTimeOffset.UtcNow : null,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.EvidenceItems.Add(evidenceItem);

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_CREATED",
                $"Evidence item created. Type={evidenceType}; Status={evidenceStatus}");

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/claim-workspaces/{workspaceId}/evidence-items/{evidenceItem.Id}",
                ToEvidenceItemResponse(evidenceItem));
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items/{evidenceItemId:guid}", async (
            Guid workspaceId,
            Guid evidenceItemId,
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

            var evidenceItem = await db.EvidenceItems
                .FirstOrDefaultAsync(x =>
                    x.Id == evidenceItemId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (evidenceItem is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToEvidenceItemResponse(evidenceItem));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items/{evidenceItemId:guid}", async (
            Guid workspaceId,
            Guid evidenceItemId,
            HttpRequest request,
            FirebaseAuthService firebaseAuthService,
            VirtualAdvocateDbContext db,
            UpdateEvidenceItemRequest input) =>
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

            var evidenceItem = await db.EvidenceItems
                .FirstOrDefaultAsync(x =>
                    x.Id == evidenceItemId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (evidenceItem is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.EvidenceType))
            {
                var evidenceType = NormaliseEvidenceType(input.EvidenceType);

                if (!IsValidEvidenceType(evidenceType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid evidence type.",
                        allowedValues = GetAllowedEvidenceTypes()
                    });
                }

                evidenceItem.EvidenceType = evidenceType;
            }

            if (!string.IsNullOrWhiteSpace(input.EvidenceStatus))
            {
                var evidenceStatus = NormaliseEvidenceStatus(input.EvidenceStatus);

                if (!IsValidEvidenceStatus(evidenceStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid evidence status.",
                        allowedValues = GetAllowedEvidenceStatuses()
                    });
                }

                evidenceItem.EvidenceStatus = evidenceStatus;

                if (evidenceStatus == "UPLOADED" && evidenceItem.UploadedAt is null)
                {
                    evidenceItem.UploadedAt = DateTimeOffset.UtcNow;
                }
            }

            evidenceItem.OriginalFileName = input.OriginalFileName ?? evidenceItem.OriginalFileName;
            evidenceItem.StoragePath = input.StoragePath ?? evidenceItem.StoragePath;
            evidenceItem.FileType = input.FileType ?? evidenceItem.FileType;
            evidenceItem.FileSize = input.FileSize ?? evidenceItem.FileSize;
            evidenceItem.DocumentDate = input.DocumentDate ?? evidenceItem.DocumentDate;
            evidenceItem.ProviderName = input.ProviderName ?? evidenceItem.ProviderName;
            evidenceItem.UserNotes = input.UserNotes ?? evidenceItem.UserNotes;
            evidenceItem.AiSummary = input.AiSummary ?? evidenceItem.AiSummary;
            evidenceItem.UserConfirmedSummary = input.UserConfirmedSummary ?? evidenceItem.UserConfirmedSummary;

            if (input.UsedInGeneratedPack.HasValue)
            {
                evidenceItem.UsedInGeneratedPack = input.UsedInGeneratedPack.Value;
            }

            evidenceItem.UpdatedAt = DateTimeOffset.UtcNow;

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_UPDATED",
                $"Evidence item updated. EvidenceItemId={evidenceItem.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToEvidenceItemResponse(evidenceItem));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items/{evidenceItemId:guid}", async (
            Guid workspaceId,
            Guid evidenceItemId,
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

            var evidenceItem = await db.EvidenceItems
                .FirstOrDefaultAsync(x =>
                    x.Id == evidenceItemId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (evidenceItem is null)
            {
                return Results.NotFound();
            }

            evidenceItem.Status = "ARCHIVED";
            evidenceItem.UpdatedAt = DateTimeOffset.UtcNow;

            AddAuditEvent(
                db,
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_ARCHIVED",
                $"Evidence item archived. EvidenceItemId={evidenceItem.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = evidenceItem.Id,
                status = evidenceItem.Status,
                archived = true
            });
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/audit-events", async (
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

            var rows = await db.AuditEvents
                .Where(x => x.UserId == user.Id && x.ClaimWorkspaceId == workspaceId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(200)
                .ToListAsync();

            return Results.Ok(rows.Select(ToAuditEventResponse).ToList());
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/audit-events/{auditEventId:guid}", async (
            Guid workspaceId,
            Guid auditEventId,
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

            var auditEvent = await db.AuditEvents
                .FirstOrDefaultAsync(x =>
                    x.Id == auditEventId &&
                    x.UserId == user.Id &&
                    x.ClaimWorkspaceId == workspaceId);

            if (auditEvent is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToAuditEventResponse(auditEvent));
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

    private static void AddAuditEvent(
        VirtualAdvocateDbContext db,
        HttpRequest request,
        Guid userId,
        Guid workspaceId,
        string eventType,
        string? eventDetail)
    {
        db.AuditEvents.Add(new AuditEvent
        {
            UserId = userId,
            ClaimWorkspaceId = workspaceId,
            EventType = eventType,
            EventDetail = eventDetail,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            ClientType = request.Headers.UserAgent.ToString(),
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    private static object ToEvidenceItemResponse(EvidenceItem evidenceItem)
    {
        return new
        {
            id = evidenceItem.Id,
            claimWorkspaceId = evidenceItem.ClaimWorkspaceId,
            conditionId = evidenceItem.ConditionId,
            evidenceType = evidenceItem.EvidenceType,
            evidenceStatus = evidenceItem.EvidenceStatus,
            originalFileName = evidenceItem.OriginalFileName,
            storagePath = evidenceItem.StoragePath,
            fileType = evidenceItem.FileType,
            fileSize = evidenceItem.FileSize,
            documentDate = evidenceItem.DocumentDate,
            providerName = evidenceItem.ProviderName,
            userNotes = evidenceItem.UserNotes,
            aiSummary = evidenceItem.AiSummary,
            userConfirmedSummary = evidenceItem.UserConfirmedSummary,
            usedInGeneratedPack = evidenceItem.UsedInGeneratedPack,
            uploadedAt = evidenceItem.UploadedAt,
            status = evidenceItem.Status,
            createdAt = evidenceItem.CreatedAt,
            updatedAt = evidenceItem.UpdatedAt
        };
    }

    private static object ToAuditEventResponse(AuditEvent auditEvent)
    {
        return new
        {
            id = auditEvent.Id,
            userId = auditEvent.UserId,
            claimWorkspaceId = auditEvent.ClaimWorkspaceId,
            eventType = auditEvent.EventType,
            eventDetail = auditEvent.EventDetail,
            ipAddress = auditEvent.IpAddress,
            clientType = auditEvent.ClientType,
            createdAt = auditEvent.CreatedAt
        };
    }

    private static string NormaliseEvidenceType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseEvidenceStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "LISTED_NOT_UPLOADED" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidEvidenceType(string value)
    {
        return GetAllowedEvidenceTypes().Contains(value);
    }

    private static bool IsValidEvidenceStatus(string value)
    {
        return GetAllowedEvidenceStatuses().Contains(value);
    }

    private static string[] GetAllowedEvidenceTypes()
    {
        return new[]
        {
            "DVA_DECISION_LETTER",
            "PREVIOUS_PI_ASSESSMENT",
            "DCP_ASSESSMENT",
            "MEDICAL_REPORT",
            "SPECIALIST_REPORT",
            "IMAGING_REPORT",
            "MEDICATION_LIST",
            "TREATMENT_SUMMARY",
            "SERVICE_DOCUMENT",
            "PERSONAL_STATEMENT",
            "FUNCTIONAL_IMPACT_NOTES",
            "APPOINTMENT_NOTES",
            "OTHER"
        };
    }

    private static string[] GetAllowedEvidenceStatuses()
    {
        return new[]
        {
            "MISSING",
            "LISTED_NOT_UPLOADED",
            "UPLOADED",
            "REVIEWED",
            "CONFIRMED",
            "NOT_APPLICABLE"
        };
    }
}

public sealed record CreateEvidenceItemRequest(
    string? EvidenceType,
    string? EvidenceStatus,
    string? OriginalFileName,
    string? StoragePath,
    string? FileType,
    long? FileSize,
    DateOnly? DocumentDate,
    string? ProviderName,
    string? UserNotes,
    string? AiSummary,
    string? UserConfirmedSummary,
    bool? UsedInGeneratedPack
);

public sealed record UpdateEvidenceItemRequest(
    string? EvidenceType,
    string? EvidenceStatus,
    string? OriginalFileName,
    string? StoragePath,
    string? FileType,
    long? FileSize,
    DateOnly? DocumentDate,
    string? ProviderName,
    string? UserNotes,
    string? AiSummary,
    string? UserConfirmedSummary,
    bool? UsedInGeneratedPack
);
