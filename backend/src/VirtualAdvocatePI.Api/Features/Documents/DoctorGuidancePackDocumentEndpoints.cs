using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Google.Cloud.Storage.V1;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Documents;

public static class DoctorGuidancePackDocumentEndpoints
{
    private const string DocxContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    private const string PdfContentType = "application/pdf";

    public static IEndpointRouteBuilder MapDoctorGuidancePackDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/generated-documents/doctor-guidance-pack", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            GenerateDoctorGuidancePackRequest input,
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

            var approvedDoctorDrafts = await db.AiDrafts
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    x.ReviewStatus == "APPROVED" &&
                    (x.DraftType == "DOCTOR_QUESTIONS" ||
                     x.DraftType == "EVIDENCE_GAP_SUMMARY" ||
                     x.DraftType == "DOCTOR_REQUEST_LETTER"))
                .OrderBy(x => x.DraftType)
                .ThenByDescending(x => x.UpdatedAt)
                .ToListAsync(cancellationToken);

            var excludedUnapprovedDoctorDraftCount = await db.AiDrafts
                .CountAsync(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    x.ReviewStatus != "APPROVED" &&
                    (x.DraftType == "DOCTOR_QUESTIONS" ||
                     x.DraftType == "EVIDENCE_GAP_SUMMARY" ||
                     x.DraftType == "DOCTOR_REQUEST_LETTER"),
                    cancellationToken);

            var documentId = Guid.NewGuid();
            var generatedAt = DateTimeOffset.UtcNow;

            var existingDocumentCount = await db.GeneratedDocuments
                .CountAsync(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.DocumentType == "DOCTOR_GUIDANCE_PACK" &&
                    x.Status != "ARCHIVED",
                    cancellationToken);

            var documentVersionNumber = existingDocumentCount + 1;
            var documentVersion = $"v{documentVersionNumber:000}";

            var docxBytes = BuildDoctorGuidancePackDocx(
                workspace,
                activeConditions,
                acceptedHistory,
                questionResponses,
                evidenceItems,
                evidenceGaps,
                approvedDoctorDrafts,
                generatedAt,
                documentVersion);

            var pdfBytes = BuildDoctorGuidancePackPdf(
                workspace,
                activeConditions,
                acceptedHistory,
                questionResponses,
                evidenceItems,
                evidenceGaps,
                approvedDoctorDrafts,
                generatedAt,
                documentVersion);

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

            var docxObjectName =
                $"generated-documents/{workspaceId}/doctor-guidance-pack/{documentVersion}/{documentId}/doctor-guidance-pack-{safeTitle}-{documentVersion}.docx";

            var pdfObjectName =
                $"generated-documents/{workspaceId}/doctor-guidance-pack/{documentVersion}/{documentId}/doctor-guidance-pack-{safeTitle}-{documentVersion}.pdf";

            await using var docxUploadStream = new MemoryStream(docxBytes);
            await using var pdfUploadStream = new MemoryStream(pdfBytes);

            var storageClient = await StorageClient.CreateAsync();

            await storageClient.UploadObjectAsync(
                bucketName,
                docxObjectName,
                DocxContentType,
                docxUploadStream,
                cancellationToken: cancellationToken);

            await storageClient.UploadObjectAsync(
                bucketName,
                pdfObjectName,
                PdfContentType,
                pdfUploadStream,
                cancellationToken: cancellationToken);

            var docxStoragePath = $"gs://{bucketName}/{docxObjectName}";
            var pdfStoragePath = $"gs://{bucketName}/{pdfObjectName}";

            var generatedDocument = new GeneratedDocument
            {
                Id = documentId,
                ClaimWorkspaceId = workspaceId,
                DocumentType = "DOCTOR_GUIDANCE_PACK",
                DocumentStatus = "GENERATED",
                DocxStoragePath = docxStoragePath,
                PdfStoragePath = pdfStoragePath,
                TemplateVersion = $"doctor-guidance-pack-docx-pdf-v1-{documentVersion}",
                IncludedAiDraftIds = string.Join(",", approvedDoctorDrafts.Select(x => x.Id)),
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
                "DOCTOR_GUIDANCE_PACK_CREATED",
                $"Doctor Guidance Pack generated. DocumentId={generatedDocument.Id}; Version={documentVersion}; ApprovedDoctorDraftCount={approvedDoctorDrafts.Count}; ExcludedUnapprovedDoctorDraftCount={excludedUnapprovedDoctorDraftCount}");

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "DOCTOR_GUIDANCE_PACK_REVIEWED_ONLY_ENFORCED",
                $"Doctor Guidance Pack reviewed-only rule enforced. DocumentId={generatedDocument.Id}; IncludedApprovedDoctorDraftCount={approvedDoctorDrafts.Count}; ExcludedUnapprovedDoctorDraftCount={excludedUnapprovedDoctorDraftCount}");

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                document = ToGeneratedDocumentResponse(generatedDocument),
                generated = true,
                docxStoragePath,
                pdfStoragePath,
                documentVersion,
                includedApprovedDoctorDraftCount = approvedDoctorDrafts.Count,
                excludedUnapprovedDoctorDraftCount,
                activeConditionCount = activeConditions.Count,
                evidenceItemCount = evidenceItems.Count,
                evidenceGapCount = evidenceGaps.Count,
                reviewedOnlyRule = "Only approved doctor guidance AI drafts and active workspace records were included.",
                safety = new
                {
                    preparationSupportOnly = true,
                    reviewedOnlyDoctorDrafts = true,
                    approvedDoctorDraftsOnly = true,
                    legalAdvice = false,
                    medicalAdvice = false,
                    diagnosis = false,
                    tellsDoctorWhatOpinionToProvide = false,
                    pressuresDoctor = false,
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

    private static byte[] BuildDoctorGuidancePackDocx(
        ClaimWorkspace workspace,
        List<ClaimCondition> activeConditions,
        List<AcceptedConditionHistory> acceptedHistory,
        List<QuestionResponse> questionResponses,
        List<EvidenceItem> evidenceItems,
        List<EvidenceGap> evidenceGaps,
        List<AiDraft> approvedDoctorDrafts,
        DateTimeOffset generatedAt,
        string documentVersion)
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

            AddHeading(body, "Virtual Advocate PI - Doctor Guidance Pack", 28);
            AddParagraph(body, $"Version: {documentVersion}");
            AddParagraph(body, $"Generated: {generatedAt:yyyy-MM-dd HH:mm zzz}");
            AddBlankLine(body);

            AddHeading(body, "Doctor-specific disclaimer", 20);
            AddParagraph(body, "This document is preparation support only. It does not provide legal advice, medical advice, diagnosis or a DVA decision. It does not tell a doctor what opinion to provide, does not pressure a doctor to support a claim, does not calculate impairment points, does not estimate compensation and does not guarantee a claim outcome.");
            AddBlankLine(body);

            AddHeading(body, "Reviewed-only inclusion rule", 20);
            AddParagraph(body, "This export includes active workspace records only. Doctor guidance AI draft content is included only when the draft has been marked APPROVED. Unapproved, rejected, archived or still-reviewing doctor guidance drafts are excluded from this pack.");
            AddBlankLine(body);

            AddHeading(body, "Appointment purpose", 20);
            AddParagraph(body, "Use this pack to prepare respectful questions and discussion points for a doctor, specialist or treating provider. The questions should help clarify the current clinical picture, symptoms, treatment, medication, side effects, functional impact and useful documents to request.");
            AddBlankLine(body);

            AddHeading(body, "Workspace summary", 20);
            AddParagraph(body, $"Workspace title: {workspace.WorkspaceTitle}");
            AddParagraph(body, $"Claim framework: {workspace.ClaimFramework}");
            AddParagraph(body, $"Claim scenario: {workspace.ClaimScenario}");
            AddBlankLine(body);

            AddHeading(body, "Conditions to discuss", 20);

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

            AddHeading(body, "Evidence and reports to discuss", 20);

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

                    AddBullet(body, $"{item.EvidenceType}; Status={item.EvidenceStatus}; File={item.OriginalFileName ?? "not recorded"}; Provider/source={item.ProviderName ?? "not recorded"}; Document date={item.DocumentDate}; Upload status={uploadStatus}; Notes={item.UserNotes ?? "none"}");
                }
            }

            AddBlankLine(body);

            AddHeading(body, "Evidence gaps and follow-up items", 20);

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

            AddHeading(body, "Approved doctor guidance material", 20);

            if (approvedDoctorDrafts.Count == 0)
            {
                AddParagraph(body, "No approved doctor guidance drafts are available for inclusion yet.");
            }
            else
            {
                foreach (var draft in approvedDoctorDrafts)
                {
                    AddHeading(body, FormatLabel(draft.DraftType), 18);
                    AddMultilineText(body, string.IsNullOrWhiteSpace(draft.UserEditedText)
                        ? draft.DraftText
                        : draft.UserEditedText);
                    AddBlankLine(body);
                }
            }

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

            AddHeading(body, "Doctor guidance review checklist", 20);
            AddBullet(body, "The questions are polite and neutral.");
            AddBullet(body, "The wording does not pressure the doctor.");
            AddBullet(body, "The wording does not tell the doctor what opinion to give.");
            AddBullet(body, "The wording asks for clinically appropriate information only.");
            AddBullet(body, "The wording does not ask the doctor to make a DVA decision.");
            AddBullet(body, "The material does not guarantee a claim outcome.");
            AddBlankLine(body);

            AddHeading(body, "Appointment notes", 20);
            AddParagraph(body, "Doctor comments: ______________________________");
            AddParagraph(body, "Follow-up actions: _____________________________");
            AddParagraph(body, "Documents requested: ___________________________");
            AddParagraph(body, "Next appointment: ______________________________");

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    private static byte[] BuildDoctorGuidancePackPdf(
        ClaimWorkspace workspace,
        List<ClaimCondition> activeConditions,
        List<AcceptedConditionHistory> acceptedHistory,
        List<QuestionResponse> questionResponses,
        List<EvidenceItem> evidenceItems,
        List<EvidenceGap> evidenceGaps,
        List<AiDraft> approvedDoctorDrafts,
        DateTimeOffset generatedAt,
        string documentVersion)
    {
        var lines = new List<string>
        {
            "Virtual Advocate PI - Doctor Guidance Pack",
            $"Version: {documentVersion}",
            $"Generated: {generatedAt:yyyy-MM-dd HH:mm zzz}",
            "",
            "Doctor-specific disclaimer",
            "This document is preparation support only.",
            "It does not provide legal advice, medical advice, diagnosis or a DVA decision.",
            "It does not tell a doctor what opinion to provide and does not pressure a doctor.",
            "",
            "Reviewed-only inclusion rule",
            "Only active workspace records are included.",
            "Only APPROVED doctor guidance AI drafts are included.",
            "",
            "Workspace summary",
            $"Workspace title: {workspace.WorkspaceTitle}",
            $"Claim framework: {workspace.ClaimFramework}",
            $"Claim scenario: {workspace.ClaimScenario}",
            "",
            "Conditions to discuss"
        };

        foreach (var condition in activeConditions)
        {
            lines.Add($"- {condition.ConditionName} | Diagnosis status: {condition.DiagnosisStatus} | Primary: {condition.IsPrimaryCondition}");
        }

        lines.Add("");
        lines.Add("Evidence and reports to discuss");

        foreach (var item in evidenceItems)
        {
            var uploadStatus = !string.IsNullOrWhiteSpace(item.StoragePath) && item.UploadedAt.HasValue
                ? "uploaded"
                : "listed, not uploaded";

            lines.Add($"- {item.EvidenceType}; Status={item.EvidenceStatus}; File={item.OriginalFileName ?? "not recorded"}; Provider={item.ProviderName ?? "not recorded"}; Upload status={uploadStatus}; Notes={item.UserNotes ?? "none"}");
        }

        lines.Add("");
        lines.Add("Evidence gaps and follow-up items");

        foreach (var gap in evidenceGaps)
        {
            lines.Add($"- {gap.GapType}; Severity={gap.Severity}; {gap.PlainEnglishExplanation} Suggested next step: {gap.SuggestedNextStep}");
        }

        lines.Add("");
        lines.Add("Approved doctor guidance material");

        if (approvedDoctorDrafts.Count == 0)
        {
            lines.Add("No approved doctor guidance drafts are available for inclusion yet.");
        }
        else
        {
            foreach (var draft in approvedDoctorDrafts)
            {
                lines.Add("");
                lines.Add(FormatLabel(draft.DraftType));
                lines.AddRange((string.IsNullOrWhiteSpace(draft.UserEditedText) ? draft.DraftText : draft.UserEditedText)
                    .Replace("\r\n", "\n")
                    .Split('\n'));
            }
        }

        lines.Add("");
        lines.Add("Doctor guidance review checklist");
        lines.Add("- The questions are polite and neutral.");
        lines.Add("- The wording does not pressure the doctor.");
        lines.Add("- The wording does not tell the doctor what opinion to give.");
        lines.Add("- The wording does not ask the doctor to make a DVA decision.");
        lines.Add("");
        lines.Add("Appointment notes");
        lines.Add("Doctor comments: ______________________________");
        lines.Add("Follow-up actions: _____________________________");
        lines.Add("Documents requested: ___________________________");
        lines.Add("Next appointment: ______________________________");

        return BuildSimplePdfFromLines(lines);
    }

    private static byte[] BuildSimplePdfFromLines(List<string> sourceLines)
    {
        var wrappedLines = sourceLines
            .SelectMany(line => WrapPdfLine(NormalisePdfText(line), 95))
            .ToList();

        if (wrappedLines.Count == 0)
        {
            wrappedLines.Add("Virtual Advocate PI - Doctor Guidance Pack");
        }

        var pages = wrappedLines
            .Chunk(42)
            .Select(chunk => chunk.ToList())
            .ToList();

        var fontObjectId = 3 + pages.Count * 2;
        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>"
        };

        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(index => $"{3 + index * 2} 0 R"));
        objects.Add($"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>");

        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            var pageObjectId = 3 + pageIndex * 2;
            var contentObjectId = pageObjectId + 1;
            var streamContent = BuildPdfPageContent(pages[pageIndex]);
            var streamLength = Encoding.ASCII.GetByteCount(streamContent);

            objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontObjectId} 0 R >> >> /Contents {contentObjectId} 0 R >>");
            objects.Add($"<< /Length {streamLength} >>\nstream\n{streamContent}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");

        var pdf = new StringBuilder();
        var offsets = new List<int> { 0 };

        pdf.Append("%PDF-1.4\n");

        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString()));
            pdf.Append($"{index + 1} 0 obj\n");
            pdf.Append(objects[index]);
            pdf.Append("\nendobj\n");
        }

        var xrefStart = Encoding.ASCII.GetByteCount(pdf.ToString());

        pdf.Append("xref\n");
        pdf.Append($"0 {objects.Count + 1}\n");
        pdf.Append("0000000000 65535 f \n");

        foreach (var offset in offsets.Skip(1))
        {
            pdf.Append($"{offset:0000000000} 00000 n \n");
        }

        pdf.Append("trailer\n");
        pdf.Append($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
        pdf.Append("startxref\n");
        pdf.Append(xrefStart);
        pdf.Append("\n%%EOF");

        return Encoding.ASCII.GetBytes(pdf.ToString());
    }

    private static string BuildPdfPageContent(List<string> lines)
    {
        var builder = new StringBuilder();

        builder.AppendLine("BT");
        builder.AppendLine("/F1 10 Tf");
        builder.AppendLine("50 790 Td");
        builder.AppendLine("14 TL");

        foreach (var line in lines)
        {
            builder.AppendLine($"({EscapePdfText(line)}) Tj");
            builder.AppendLine("T*");
        }

        builder.AppendLine("ET");

        return builder.ToString();
    }

    private static IEnumerable<string> WrapPdfLine(string line, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            yield return string.Empty;
            yield break;
        }

        var remaining = line.Trim();

        while (remaining.Length > maxLength)
        {
            var splitAt = remaining.LastIndexOf(' ', maxLength);

            if (splitAt <= 0)
            {
                splitAt = maxLength;
            }

            yield return remaining[..splitAt].Trim();

            remaining = remaining[splitAt..].Trim();
        }

        if (remaining.Length > 0)
        {
            yield return remaining;
        }
    }

    private static string EscapePdfText(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
    }

    private static string NormalisePdfText(string value)
    {
        var builder = new StringBuilder();

        foreach (var ch in value)
        {
            if (ch is >= ' ' and <= '~')
            {
                builder.Append(ch);
            }
            else if (ch == '\t')
            {
                builder.Append(' ');
            }
            else
            {
                builder.Append('-');
            }
        }

        return builder.ToString();
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
            ? "doctor-guidance-pack"
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

public sealed record GenerateDoctorGuidancePackRequest(
    string? Notes
);