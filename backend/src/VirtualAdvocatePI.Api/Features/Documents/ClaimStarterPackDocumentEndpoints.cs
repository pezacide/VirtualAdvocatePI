using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Documents;

public static class ClaimStarterPackDocumentEndpoints
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    public static IEndpointRouteBuilder MapClaimStarterPackDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/claim-starter-pack", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            GenerateClaimStarterPackRequest input,
            CancellationToken cancellationToken) =>
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

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == workspaceId && x.Status != "ARCHIVED", cancellationToken);

            if (workspace is null)
            {
                return Results.NotFound();
            }

            var activeConditions = await db.ClaimConditions
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderBy(x => x.ConditionName)
                .ToListAsync(cancellationToken);

            var acceptedHistory = await db.AcceptedConditionHistories
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync(cancellationToken);

            var questionResponses = await db.QuestionResponses
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderBy(x => x.QuestionGroup)
                .ThenBy(x => x.QuestionKey)
                .ToListAsync(cancellationToken);

            var evidenceItems = await db.EvidenceItems
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderBy(x => x.EvidenceType)
                .ThenBy(x => x.OriginalFileName)
                .ToListAsync(cancellationToken);

            var evidenceGaps = await db.EvidenceGaps
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.Severity)
                .ThenBy(x => x.GapType)
                .ToListAsync(cancellationToken);

            var approvedAiDrafts = await db.AiDrafts
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    x.ReviewStatus == "APPROVED")
                .OrderBy(x => x.DraftType)
                .ThenByDescending(x => x.UpdatedAt)
                .ToListAsync(cancellationToken);

            var documentId = Guid.NewGuid();
            var generatedAt = DateTimeOffset.UtcNow;

            var docxBytes = BuildClaimStarterPackDocx(
                workspace,
                activeConditions,
                acceptedHistory,
                questionResponses,
                evidenceItems,
                evidenceGaps,
                approvedAiDrafts,
                generatedAt);

            var bucketName = GetDocumentBucketName();

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                return Results.BadRequest(new
                {
                    error = "No document storage bucket is configured.",
                    expectedEnvironmentVariables = new[]
                    {
                        "VAPI_DOCUMENT_BUCKET",
                        "DOCUMENT_BUCKET_NAME",
                        "VIRTUAL_ADVOCATE_DOCUMENT_BUCKET",
                        "EVIDENCE_BUCKET_NAME",
                        "GOOGLE_CLOUD_STORAGE_BUCKET"
                    }
                });
            }

            var safeTitle = CreateSafeFileName(workspace.WorkspaceTitle);
            var objectName =
                $"generated-documents/{workspaceId}/{documentId}/claim-starter-pack-{safeTitle}-v1.docx";

            await using var uploadStream = new MemoryStream(docxBytes);

            var storageClient = await StorageClient.CreateAsync();

            await storageClient.UploadObjectAsync(
                bucketName,
                objectName,
                DocxContentType,
                uploadStream,
                cancellationToken: cancellationToken);

            var docxStoragePath = $"gs://{bucketName}/{objectName}";

            var generatedDocument = new GeneratedDocument
            {
                Id = documentId,
                ClaimWorkspaceId = workspaceId,
                DocumentType = "CLAIM_STARTER_PACK",
                DocumentStatus = "GENERATED",
                DocxStoragePath = docxStoragePath,
                PdfStoragePath = null,
                TemplateVersion = "claim-starter-pack-docx-v1",
                IncludedAiDraftIds = string.Join(",", approvedAiDrafts.Select(x => x.Id)),
                GeneratedAt = generatedAt,
                DownloadedAt = null,
                Status = "ACTIVE",
                CreatedAt = generatedAt,
                UpdatedAt = generatedAt
            };

            db.GeneratedDocuments.Add(generatedDocument);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "GENERATED_DOCUMENT_CREATED",
                $"Claim Starter Pack DOCX generated. DocumentId={generatedDocument.Id}; ApprovedAiDraftCount={approvedAiDrafts.Count}; ConditionCount={activeConditions.Count}; EvidenceCount={evidenceItems.Count}");

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "CLAIM_STARTER_PACK_DOCX_GENERATED",
                $"Claim Starter Pack DOCX stored. DocumentId={generatedDocument.Id}; StoragePath={docxStoragePath}");

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                document = ToGeneratedDocumentResponse(generatedDocument),
                generated = true,
                docxStoragePath,
                includedAiDraftCount = approvedAiDrafts.Count,
                activeConditionCount = activeConditions.Count,
                evidenceItemCount = evidenceItems.Count,
                evidenceGapCount = evidenceGaps.Count,
                safety = new
                {
                    preparationSupportOnly = true,
                    reviewedOnlyAiDrafts = true,
                    approvedAiDraftsOnly = true,
                    legalAdvice = false,
                    medicalAdvice = false,
                    dvaDecision = false,
                    impairmentCalculation = false,
                    compensationEstimate = false,
                    outcomeGuarantee = false,
                    submittedToDva = false
                }
            });
        });

        return app;
    }

    private static byte[] BuildClaimStarterPackDocx(
        ClaimWorkspace workspace,
        List<ClaimCondition> activeConditions,
        List<AcceptedConditionHistory> acceptedHistory,
        List<QuestionResponse> questionResponses,
        List<EvidenceItem> evidenceItems,
        List<EvidenceGap> evidenceGaps,
        List<AiDraft> approvedAiDrafts,
        DateTimeOffset generatedAt)
    {
        using var stream = new MemoryStream();

        using (var document = WordprocessingDocument.Create(
            stream,
            WordprocessingDocumentType.Document,
            true))
        {
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document(new Body());

            var body = mainPart.Document.Body!;

            AddHeading(body, "Virtual Advocate PI - Claim Starter Pack", 28);
            AddParagraph(body, "Preparation support only.");
            AddParagraph(body, $"Generated: {generatedAt:yyyy-MM-dd HH:mm zzz}");
            AddBlankLine(body);

            AddHeading(body, "Important safety note", 20);
            AddParagraph(body, "This document is preparation support only. It does not provide legal advice, medical advice or a DVA decision. It does not submit anything to DVA, calculate impairment points, estimate compensation or guarantee a claim outcome. Please review all content before using it.");
            AddBlankLine(body);

            AddHeading(body, "Workspace summary", 20);
            AddParagraph(body, $"Workspace title: {workspace.WorkspaceTitle}");
            AddParagraph(body, $"Claim framework: {workspace.ClaimFramework}");
            AddParagraph(body, $"Claim scenario: {workspace.ClaimScenario}");
            AddParagraph(body, $"Generated pack status: {workspace.GeneratedPackStatus}");
            AddBlankLine(body);

            AddHeading(body, "Conditions included", 20);

            if (activeConditions.Count == 0)
            {
                AddParagraph(body, "No active conditions were found in this workspace.");
            }
            else
            {
                foreach (var condition in activeConditions)
                {
                    AddBullet(body, $"{condition.ConditionName} | Diagnosis status: {condition.DiagnosisStatus} | Primary condition: {condition.IsPrimaryCondition}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Accepted-condition history", 20);

            if (acceptedHistory.Count == 0)
            {
                AddParagraph(body, "No active accepted-condition history has been recorded.");
            }
            else
            {
                foreach (var history in acceptedHistory)
                {
                    AddBullet(body, $"ConditionId={history.ConditionId}; Previously accepted by DVA={history.PreviouslyAcceptedByDva}; Original Act={history.OriginalAct}; Previous compensation={history.PreviousCompensationReceived}; Worsening claimed={history.WorseningClaimed}; Notes={history.WorseningSummary}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Approved AI draft content", 20);

            if (approvedAiDrafts.Count == 0)
            {
                AddParagraph(body, "No approved AI drafts are available for inclusion yet. AI draft content must be reviewed and approved before it is included.");
            }
            else
            {
                foreach (var draft in approvedAiDrafts)
                {
                    AddHeading(body, FormatLabel(draft.DraftType), 18);
                    AddMultilineText(body, string.IsNullOrWhiteSpace(draft.UserEditedText)
                        ? draft.DraftText
                        : draft.UserEditedText);

                    if (!string.IsNullOrWhiteSpace(draft.SourceReferences))
                    {
                        AddParagraph(body, $"Source references: {draft.SourceReferences}");
                    }

                    AddBlankLine(body);
                }
            }

            AddHeading(body, "Evidence list", 20);

            if (evidenceItems.Count == 0)
            {
                AddParagraph(body, "No active evidence items have been recorded.");
            }
            else
            {
                foreach (var item in evidenceItems)
                {
                    var uploadStatus = !string.IsNullOrWhiteSpace(item.StoragePath) && item.UploadedAt.HasValue
                        ? "uploaded"
                        : "listed, not uploaded";

                    AddBullet(body, $"{item.EvidenceType}; Status={item.EvidenceStatus}; File={item.OriginalFileName ?? "not recorded"}; Provider/source={item.ProviderName ?? "not recorded"}; Document date={item.DocumentDate}; Upload status={uploadStatus}; Used in pack={item.UsedInGeneratedPack}; Notes={item.UserNotes ?? "none"}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Evidence gaps and follow-up", 20);

            if (evidenceGaps.Count == 0)
            {
                AddParagraph(body, "No active evidence gaps are currently recorded.");
            }
            else
            {
                foreach (var gap in evidenceGaps)
                {
                    AddBullet(body, $"{gap.GapType}; Status={gap.GapStatus}; Severity={gap.Severity}; {gap.PlainEnglishExplanation} Suggested next step: {gap.SuggestedNextStep}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Guided preparation answers", 20);

            if (questionResponses.Count == 0)
            {
                AddParagraph(body, "No active guided preparation answers were found.");
            }
            else
            {
                foreach (var response in questionResponses)
                {
                    AddBullet(body, $"{response.QuestionGroup} / {response.QuestionKey}: {response.AnswerText}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Review and sign-off", 20);
            AddParagraph(body, "Before using this pack, review each section for accuracy, completeness and your own wording.");
            AddParagraph(body, "Date reviewed: ____________________");
            AddParagraph(body, "Notes: ____________________________");

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static void AddHeading(Body body, string text, int size)
    {
        var paragraph = new Paragraph();
        var run = new Run();

        run.Append(new RunProperties(
            new Bold(),
            new FontSize { Val = (size * 2).ToString() }));

        run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private static void AddParagraph(Body body, string? text)
    {
        var paragraph = new Paragraph();
        var run = new Run();
        run.Append(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });
        paragraph.Append(run);
        body.Append(paragraph);
    }

    private static void AddBullet(Body body, string text)
    {
        AddParagraph(body, $"• {text}");
    }

    private static void AddBlankLine(Body body)
    {
        AddParagraph(body, string.Empty);
    }

    private static void AddMultilineText(Body body, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            AddParagraph(body, "No reviewed text available.");
            return;
        }

        foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
        {
            AddParagraph(body, line);
        }
    }

    private static string FormatLabel(string value)
    {
        return value
            .Replace("_", " ")
            .ToLowerInvariant();
    }

    private static string CreateSafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(value.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        safe = safe.Trim().Replace(" ", "-").ToLowerInvariant();

        return string.IsNullOrWhiteSpace(safe)
            ? "claim-starter-pack"
            : safe;
    }

    private static string? GetDocumentBucketName()
    {
        return Environment.GetEnvironmentVariable("VAPI_DOCUMENT_BUCKET")
            ?? Environment.GetEnvironmentVariable("DOCUMENT_BUCKET_NAME")
            ?? Environment.GetEnvironmentVariable("VIRTUAL_ADVOCATE_DOCUMENT_BUCKET")
            ?? Environment.GetEnvironmentVariable("EVIDENCE_BUCKET_NAME")
            ?? Environment.GetEnvironmentVariable("GOOGLE_CLOUD_STORAGE_BUCKET");
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
            generatedAt = document.GeneratedAt,
            downloadedAt = document.DownloadedAt,
            templateVersion = document.TemplateVersion,
            includedAiDraftIds = document.IncludedAiDraftIds,
            status = document.Status,
            createdAt = document.CreatedAt,
            updatedAt = document.UpdatedAt
        };
    }
}

public sealed record GenerateClaimStarterPackRequest(
    string? Notes
);