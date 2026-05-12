using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Documents;

public static class GeneratedDocumentEndpoints
{
    public static IEndpointRouteBuilder MapGeneratedDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents", async (
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

            var documents = await db.GeneratedDocuments
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(documents.Select(ToGeneratedDocumentResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateGeneratedDocumentRequest input) =>
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

            var documentType = NormaliseDocumentType(input.DocumentType);

            if (!IsValidDocumentType(documentType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid document type.",
                    allowedValues = GetAllowedDocumentTypes()
                });
            }

            var documentStatus = NormaliseDocumentStatus(input.DocumentStatus);

            if (!IsValidDocumentStatus(documentStatus))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid document status.",
                    allowedValues = GetAllowedDocumentStatuses()
                });
            }

            var document = new GeneratedDocument
            {
                ClaimWorkspaceId = workspaceId,
                DocumentType = documentType,
                DocumentStatus = documentStatus,
                DocxStoragePath = input.DocxStoragePath,
                PdfStoragePath = input.PdfStoragePath,
                TemplateVersion = string.IsNullOrWhiteSpace(input.TemplateVersion)
                    ? "template-v1"
                    : input.TemplateVersion.Trim(),
                IncludedAiDraftIds = input.IncludedAiDraftIds,
                GeneratedAt = documentStatus == "GENERATED" ? DateTimeOffset.UtcNow : null,
                DownloadedAt = documentStatus == "DOWNLOADED" ? DateTimeOffset.UtcNow : null,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.GeneratedDocuments.Add(document);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "GENERATED_DOCUMENT_CREATED",
                $"Generated document metadata created. DocumentType={documentType}; DocumentId={document.Id}");

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/claim-workspaces/{workspaceId}/generated-documents/{document.Id}",
                ToGeneratedDocumentResponse(document));
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/{documentId:guid}", async (
            Guid workspaceId,
            Guid documentId,
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

            var document = await db.GeneratedDocuments
                .FirstOrDefaultAsync(x =>
                    x.Id == documentId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (document is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToGeneratedDocumentResponse(document));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/{documentId:guid}", async (
            Guid workspaceId,
            Guid documentId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateGeneratedDocumentRequest input) =>
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

            var document = await db.GeneratedDocuments
                .FirstOrDefaultAsync(x =>
                    x.Id == documentId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (document is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.DocumentType))
            {
                var documentType = NormaliseDocumentType(input.DocumentType);

                if (!IsValidDocumentType(documentType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid document type.",
                        allowedValues = GetAllowedDocumentTypes()
                    });
                }

                document.DocumentType = documentType;
            }

            if (!string.IsNullOrWhiteSpace(input.DocumentStatus))
            {
                var documentStatus = NormaliseDocumentStatus(input.DocumentStatus);

                if (!IsValidDocumentStatus(documentStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid document status.",
                        allowedValues = GetAllowedDocumentStatuses()
                    });
                }

                document.DocumentStatus = documentStatus;

                if (documentStatus == "GENERATED")
                {
                    document.GeneratedAt ??= DateTimeOffset.UtcNow;
                }

                if (documentStatus == "DOWNLOADED")
                {
                    document.DownloadedAt = DateTimeOffset.UtcNow;
                }
            }

            if (input.DocxStoragePath is not null)
            {
                document.DocxStoragePath = input.DocxStoragePath;
            }

            if (input.PdfStoragePath is not null)
            {
                document.PdfStoragePath = input.PdfStoragePath;
            }

            if (!string.IsNullOrWhiteSpace(input.TemplateVersion))
            {
                document.TemplateVersion = input.TemplateVersion.Trim();
            }

            if (input.IncludedAiDraftIds is not null)
            {
                document.IncludedAiDraftIds = input.IncludedAiDraftIds;
            }

            document.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "GENERATED_DOCUMENT_UPDATED",
                $"Generated document metadata updated. DocumentType={document.DocumentType}; DocumentStatus={document.DocumentStatus}; DocumentId={document.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToGeneratedDocumentResponse(document));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/{documentId:guid}", async (
            Guid workspaceId,
            Guid documentId,
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

            var document = await db.GeneratedDocuments
                .FirstOrDefaultAsync(x =>
                    x.Id == documentId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (document is null)
            {
                return Results.NotFound();
            }

            document.Status = "ARCHIVED";
            document.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "GENERATED_DOCUMENT_ARCHIVED",
                $"Generated document archived. DocumentType={document.DocumentType}; DocumentId={document.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = document.Id,
                status = document.Status,
                archived = true
            });
        });

        return app;
    }

    private static object ToGeneratedDocumentResponse(GeneratedDocument document)
    {
        return new
        {
            id = document.Id,
            claimWorkspaceId = document.ClaimWorkspaceId,
            documentType = document.DocumentType,
            documentStatus = document.DocumentStatus,
            docxStoragePath = document.DocxStoragePath,
            pdfStoragePath = document.PdfStoragePath,
            templateVersion = document.TemplateVersion,
            includedAiDraftIds = document.IncludedAiDraftIds,
            generatedAt = document.GeneratedAt,
            downloadedAt = document.DownloadedAt,
            status = document.Status,
            createdAt = document.CreatedAt,
            updatedAt = document.UpdatedAt
        };
    }

    private static string NormaliseDocumentType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "POST_2026_PI_CLAIM_STARTER_PACK" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseDocumentStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "REQUESTED" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidDocumentType(string value)
    {
        return GetAllowedDocumentTypes().Contains(value);
    }

    private static bool IsValidDocumentStatus(string value)
    {
        return GetAllowedDocumentStatuses().Contains(value);
    }

    private static string[] GetAllowedDocumentTypes()
    {
        return new[]
        {
            "POST_2026_PI_CLAIM_STARTER_PACK",
            "DOCTOR_GUIDANCE_PACK",
            "DOCTOR_REQUEST_LETTER",
            "EVIDENCE_GAP_SUMMARY"
        };
    }

    private static string[] GetAllowedDocumentStatuses()
    {
        return new[]
        {
            "REQUESTED",
            "GENERATING",
            "GENERATED",
            "FAILED",
            "DOWNLOADED",
            "SUPERSEDED"
        };
    }
}

public sealed record CreateGeneratedDocumentRequest(
    string? DocumentType,
    string? DocumentStatus,
    string? DocxStoragePath,
    string? PdfStoragePath,
    string? TemplateVersion,
    string? IncludedAiDraftIds
);

public sealed record UpdateGeneratedDocumentRequest(
    string? DocumentType,
    string? DocumentStatus,
    string? DocxStoragePath,
    string? PdfStoragePath,
    string? TemplateVersion,
    string? IncludedAiDraftIds
);
