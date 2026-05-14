using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Evidence;

public static class EvidenceUploadEndpoints
{
    private const long MaxEvidenceUploadBytes = 25 * 1024 * 1024;

    private static readonly HashSet<string> AllowedUploadContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp",
        "image/heic",
        "image/heif",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        "application/rtf",
        "text/rtf"
    };

    private static readonly HashSet<string> AllowedUploadFileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
        ".heic",
        ".heif",
        ".doc",
        ".docx",
        ".txt",
        ".rtf"
    };

    public static IEndpointRouteBuilder MapEvidenceUploadEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/evidence-upload-url", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateEvidenceUploadUrlRequest input) =>
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

            if (string.IsNullOrWhiteSpace(input.OriginalFileName))
            {
                return Results.BadRequest(new { error = "Original file name is required." });
            }

            var uploadValidationError = ValidateUploadRequest(input);

            if (uploadValidationError is not null)
            {
                return uploadValidationError;
            }

            var bucketName = GetEvidenceBucketName();
            var evidenceType = NormaliseEvidenceType(input.EvidenceType);

            if (!IsValidEvidenceType(evidenceType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid evidence type.",
                    allowedValues = GetAllowedEvidenceTypes()
                });
            }

            var evidenceItem = new EvidenceItem
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = conditionId,
                EvidenceType = evidenceType,
                EvidenceStatus = "LISTED_NOT_UPLOADED",
                OriginalFileName = input.OriginalFileName,
                FileType = input.FileType,
                FileSize = input.FileSize,
                DocumentDate = input.DocumentDate,
                ProviderName = input.ProviderName,
                UserNotes = input.UserNotes,
                UsedInGeneratedPack = false,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.EvidenceItems.Add(evidenceItem);
            await db.SaveChangesAsync();

            var safeFileName = CreateSafeFileName(input.OriginalFileName);
            var objectName = $"evidence/{workspaceId}/{conditionId}/{evidenceItem.Id}/{safeFileName}";

            evidenceItem.StoragePath = $"gs://{bucketName}/{objectName}";
            evidenceItem.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_UPLOAD_URL_CREATED",
                $"Signed upload URL created. EvidenceItemId={evidenceItem.Id}; Type={evidenceType}");

            await db.SaveChangesAsync();

            var signer = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
            var uploadUrl = await signer.SignAsync(
                bucketName,
                objectName,
                TimeSpan.FromMinutes(15),
                HttpMethod.Put);

            return Results.Ok(new
            {
                evidenceItem = ToEvidenceItemResponse(evidenceItem),
                upload = new
                {
                    method = "PUT",
                    url = uploadUrl,
                    expiresInMinutes = 15,
                    requiredHeaders = new { },
                    note = "Upload the file directly to this URL using HTTP PUT. After upload, call mark-uploaded."
                }
            });
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items/{evidenceItemId:guid}/mark-uploaded", async (
            Guid workspaceId,
            Guid evidenceItemId,
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

            var evidenceItem = await db.EvidenceItems
                .FirstOrDefaultAsync(x =>
                    x.Id == evidenceItemId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (evidenceItem is null)
            {
                return Results.NotFound();
            }

            var bucketName = GetEvidenceBucketName();

            if (string.IsNullOrWhiteSpace(evidenceItem.StoragePath))
            {
                return Results.BadRequest(new { error = "Evidence item has no storage path." });
            }

            var objectName = GetObjectNameFromStoragePath(evidenceItem.StoragePath, bucketName);

            try
            {
                var storageClient = await StorageClient.CreateAsync();
                var storageObject = await storageClient.GetObjectAsync(bucketName, objectName);

                evidenceItem.EvidenceStatus = "UPLOADED";
                evidenceItem.UploadedAt = DateTimeOffset.UtcNow;
                evidenceItem.UpdatedAt = DateTimeOffset.UtcNow;

                if (storageObject.Size.HasValue)
                {
                    evidenceItem.FileSize = (long)storageObject.Size.Value;
                }

                auditService.AddAuditEvent(
                    request,
                    user.Id,
                    workspaceId,
                    "EVIDENCE_UPLOADED",
                    $"Evidence upload confirmed. EvidenceItemId={evidenceItem.Id}");

                await db.SaveChangesAsync();

                return Results.Ok(ToEvidenceItemResponse(evidenceItem));
            }
            catch
            {
                return Results.BadRequest(new
                {
                    error = "The uploaded object could not be found in Cloud Storage yet.",
                    evidenceItemId = evidenceItem.Id
                });
            }
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/evidence-items/{evidenceItemId:guid}/download-url", async (
            Guid workspaceId,
            Guid evidenceItemId,
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

            var evidenceItem = await db.EvidenceItems
                .FirstOrDefaultAsync(x =>
                    x.Id == evidenceItemId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (evidenceItem is null)
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(evidenceItem.StoragePath))
            {
                return Results.BadRequest(new { error = "Evidence item has no storage path." });
            }

            var bucketName = GetEvidenceBucketName();
            var objectName = GetObjectNameFromStoragePath(evidenceItem.StoragePath, bucketName);

            var signer = UrlSigner.FromCredential(GoogleCredential.GetApplicationDefault());
            var downloadUrl = await signer.SignAsync(
                bucketName,
                objectName,
                TimeSpan.FromMinutes(10),
                HttpMethod.Get);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "EVIDENCE_DOWNLOAD_URL_CREATED",
                $"Signed download URL created. EvidenceItemId={evidenceItem.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                evidenceItemId = evidenceItem.Id,
                method = "GET",
                url = downloadUrl,
                expiresInMinutes = 10
            });
        });

        return app;
    }

    private static string GetEvidenceBucketName()
    {
        var bucketName = Environment.GetEnvironmentVariable("EVIDENCE_BUCKET_NAME");

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return "dva-sop-dev-vapi-dev-evidence";
        }

        return bucketName;
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


    private static IResult? ValidateUploadRequest(CreateEvidenceUploadUrlRequest input)
    {
        var fileName = Path.GetFileName(input.OriginalFileName ?? string.Empty);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Results.BadRequest(new { error = "Original file name is required." });
        }

        if (fileName.Length > 180)
        {
            return Results.BadRequest(new { error = "File name is too long. Rename the file and try again." });
        }

        if (!input.FileSize.HasValue || input.FileSize.Value <= 0)
        {
            return Results.BadRequest(new { error = "File size is required and must be greater than zero." });
        }

        if (input.FileSize.Value > MaxEvidenceUploadBytes)
        {
            return Results.BadRequest(new
            {
                error = "File is too large.",
                maxFileSizeBytes = MaxEvidenceUploadBytes
            });
        }

        var extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension) || !AllowedUploadFileExtensions.Contains(extension))
        {
            return Results.BadRequest(new
            {
                error = "Unsupported file extension.",
                allowedExtensions = AllowedUploadFileExtensions.OrderBy(x => x).ToArray()
            });
        }

        if (!string.IsNullOrWhiteSpace(input.FileType) &&
            !AllowedUploadContentTypes.Contains(input.FileType.Trim()))
        {
            return Results.BadRequest(new
            {
                error = "Unsupported file content type.",
                allowedContentTypes = AllowedUploadContentTypes.OrderBy(x => x).ToArray()
            });
        }

        return null;
    }
    private static string CreateSafeFileName(string originalFileName)
    {
        var fileName = Path.GetFileName(originalFileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "evidence-file";
        }

        foreach (var invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '-');
        }

        return fileName.Replace(' ', '-').Trim();
    }

    private static string GetObjectNameFromStoragePath(string storagePath, string bucketName)
    {
        var prefix = $"gs://{bucketName}/";

        if (!storagePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Storage path does not match the configured evidence bucket.");
        }

        return storagePath[prefix.Length..];
    }

    private static string NormaliseEvidenceType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidEvidenceType(string value)
    {
        return GetAllowedEvidenceTypes().Contains(value);
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
}

public sealed record CreateEvidenceUploadUrlRequest(
    string? EvidenceType,
    string? OriginalFileName,
    string? FileType,
    long? FileSize,
    DateOnly? DocumentDate,
    string? ProviderName,
    string? UserNotes
);
