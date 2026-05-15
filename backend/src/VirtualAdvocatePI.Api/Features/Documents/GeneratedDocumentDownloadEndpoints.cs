using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Documents;

public static class GeneratedDocumentDownloadEndpoints
{
    public static IEndpointRouteBuilder MapGeneratedDocumentDownloadEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/{documentId:guid}/download-url", async (
            Guid workspaceId,
            Guid documentId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateGeneratedDocumentDownloadUrlRequest input) =>
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

            var format = NormaliseFormat(input.Format);

            if (format is not "DOCX" and not "PDF")
            {
                return Results.BadRequest(new
                {
                    error = "Unsupported generated document download format.",
                    allowedValues = new[] { "DOCX", "PDF" }
                });
            }

            var storagePath = format == "PDF"
                ? document.PdfStoragePath
                : document.DocxStoragePath;

            if (string.IsNullOrWhiteSpace(storagePath))
            {
                return Results.BadRequest(new
                {
                    error = $"{format} storage path is not available for this generated document.",
                    format,
                    documentId = document.Id
                });
            }

            var bucketName = GetBucketNameFromStoragePath(storagePath) ?? GetDocumentBucketName();

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return Results.BadRequest(new
                {
                    error = "No document storage bucket is configured or present in the storage path."
                });
            }

            var objectName = GetObjectNameFromStoragePath(storagePath, bucketName);

            if (string.IsNullOrWhiteSpace(objectName))
            {
                return Results.BadRequest(new
                {
                    error = "Could not read the storage object name from the generated document storage path.",
                    storagePath
                });
            }

            var signer = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());

            var downloadUrl = await signer.SignAsync(
                bucketName,
                objectName,
                TimeSpan.FromMinutes(15),
                HttpMethod.Get);

            document.DownloadedAt = DateTimeOffset.UtcNow;
            document.DocumentStatus = "DOWNLOADED";
            document.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "GENERATED_DOCUMENT_DOWNLOAD_URL_CREATED",
                $"Generated document signed download URL created. DocumentId={document.Id}; Format={format}; StoragePath={storagePath}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                documentId = document.Id,
                format,
                url = downloadUrl,
                method = "GET",
                expiresInMinutes = 15,
                storagePath,
                document = new
                {
                    id = document.Id,
                    claimWorkspaceId = document.ClaimWorkspaceId,
                    documentType = document.DocumentType,
                    documentStatus = document.DocumentStatus,
                    docxStoragePath = document.DocxStoragePath,
                    pdfStoragePath = document.PdfStoragePath,
                    generatedAt = document.GeneratedAt,
                    downloadedAt = document.DownloadedAt,
                    templateVersion = document.TemplateVersion,
                    includedAiDraftIds = document.IncludedAiDraftIds,
                    status = document.Status,
                    createdAt = document.CreatedAt,
                    updatedAt = document.UpdatedAt
                },
                safety = new
                {
                    preparationSupportOnly = true,
                    submittedToDva = false,
                    legalAdvice = false,
                    medicalAdvice = false,
                    dvaDecision = false,
                    outcomeGuarantee = false
                }
            });
        });

        return app;
    }

    private static string NormaliseFormat(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "DOCX"
            : value.Trim().ToUpperInvariant();
    }

    private static string? GetBucketNameFromStoragePath(string storagePath)
    {
        if (!storagePath.StartsWith("gs://", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var withoutScheme = storagePath["gs://".Length..];
        var slashIndex = withoutScheme.IndexOf('/');

        if (slashIndex <= 0)
        {
            return null;
        }

        return withoutScheme[..slashIndex];
    }

    private static string? GetObjectNameFromStoragePath(string storagePath, string bucketName)
    {
        var prefix = $"gs://{bucketName}/";

        if (!storagePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return storagePath[prefix.Length..];
    }

    private static string? GetDocumentBucketName()
    {
        return Environment.GetEnvironmentVariable("VAPI_DOCUMENT_BUCKET")
            ?? Environment.GetEnvironmentVariable("DOCUMENT_BUCKET_NAME")
            ?? Environment.GetEnvironmentVariable("VIRTUAL_ADVOCATE_DOCUMENT_BUCKET")
            ?? Environment.GetEnvironmentVariable("EVIDENCE_BUCKET_NAME")
            ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET");
    }
}

public sealed record CreateGeneratedDocumentDownloadUrlRequest(
    string? Format
);