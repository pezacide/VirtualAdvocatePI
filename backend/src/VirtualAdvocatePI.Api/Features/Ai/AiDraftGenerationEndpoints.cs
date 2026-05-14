using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Ai;

public static class AiDraftGenerationEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapAiDraftGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts/generate", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            GenerateAiDraftRequest input) =>
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

            if (!input.ConditionId.HasValue)
            {
                return Results.BadRequest(new
                {
                    error = "ConditionId is required for veteran statement and worsening summary drafts."
                });
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, input.ConditionId.Value))
            {
                return Results.NotFound();
            }

            var draftType = NormaliseDraftType(input.DraftType);

            if (!IsSupportedDraftType(draftType))
            {
                return Results.BadRequest(new
                {
                    error = "This endpoint currently supports VETERAN_STATEMENT and WORSENING_SUMMARY only.",
                    allowedValues = GetSupportedDraftTypes()
                });
            }

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == workspaceId && x.Status != "ARCHIVED");

            if (workspace is null)
            {
                return Results.NotFound();
            }

            var condition = await db.ClaimConditions
                .FirstOrDefaultAsync(x =>
                    x.Id == input.ConditionId.Value &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (condition is null)
            {
                return Results.NotFound();
            }

            var acceptedHistory = await db.AcceptedConditionHistories
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == condition.Id &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            var questionResponses = await db.QuestionResponses
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == condition.Id &&
                    x.Status != "ARCHIVED")
                .OrderBy(x => x.QuestionGroup)
                .ThenBy(x => x.QuestionKey)
                .ToListAsync();

            var evidenceItems = await db.EvidenceItems
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == condition.Id &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            var evidenceGaps = await db.EvidenceGaps
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == condition.Id &&
                    x.Status != "ARCHIVED")
                .OrderByDescending(x => x.Severity)
                .ThenBy(x => x.GapType)
                .ToListAsync();

            var query = input.Query?.Trim() ?? draftType;
            var userInstruction = input.UserInstruction?.Trim();
            var maxSources = Math.Clamp(input.MaxSources ?? 8, 1, 12);

            var retrievedChunks = await RetrieveChunksAsync(draftType, query, maxSources);

            var sourceReferences = retrievedChunks
                .Select((chunk, index) => new SourceReference(
                    $"S{index + 1}",
                    chunk.SourceKey,
                    chunk.CitationLabel,
                    chunk.Category,
                    chunk.SourceType,
                    chunk.ChunkKey,
                    chunk.ChunkTitle))
                .ToList();

            var draftText = draftType switch
            {
                "VETERAN_STATEMENT" => BuildVeteranStatementDraft(
                    workspace,
                    condition,
                    acceptedHistory,
                    questionResponses,
                    evidenceItems,
                    evidenceGaps,
                    sourceReferences,
                    userInstruction),

                "WORSENING_SUMMARY" => BuildWorseningSummaryDraft(
                    workspace,
                    condition,
                    acceptedHistory,
                    questionResponses,
                    evidenceItems,
                    evidenceGaps,
                    sourceReferences,
                    userInstruction),

                _ => throw new InvalidOperationException("Unsupported draft type.")
            };

            var sourceReferencesJson = JsonSerializer.Serialize(sourceReferences);

            var aiDraft = new AiDraft
            {
                Id = Guid.NewGuid(),
                ClaimWorkspaceId = workspaceId,
                ConditionId = condition.Id,
                DraftType = draftType,
                PromptVersion = "deterministic-rag-draft-v1",
                SourceReferences = sourceReferencesJson,
                DraftText = draftText,
                UserEditedText = null,
                ReviewStatus = "USER_REVIEW_REQUIRED",
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.AiDrafts.Add(aiDraft);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_DRAFT_CREATED",
                $"AI draft created. DraftType={draftType}; DraftId={aiDraft.Id}; ConditionId={condition.Id}; SourceCount={sourceReferences.Count}; ReviewStatus={aiDraft.ReviewStatus}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                aiDraft = new
                {
                    aiDraft.Id,
                    aiDraft.ClaimWorkspaceId,
                    aiDraft.ConditionId,
                    aiDraft.DraftType,
                    aiDraft.PromptVersion,
                    aiDraft.SourceReferences,
                    aiDraft.DraftText,
                    aiDraft.UserEditedText,
                    aiDraft.ReviewStatus,
                    aiDraft.Status,
                    aiDraft.CreatedAt,
                    aiDraft.UpdatedAt
                },
                sourceReferences,
                safety = new
                {
                    preparationSupportOnly = true,
                    requiresUserReview = true,
                    legalAdvice = false,
                    medicalAdvice = false,
                    diagnosis = false,
                    dvaDecision = false,
                    impairmentCalculation = false,
                    compensationEstimate = false,
                    outcomeGuarantee = false,
                    aiModelCalled = false
                }
            });
        });

        return app;
    }

    private static string BuildVeteranStatementDraft(
        ClaimWorkspace workspace,
        ClaimCondition condition,
        List<AcceptedConditionHistory> acceptedHistory,
        List<QuestionResponse> questionResponses,
        List<EvidenceItem> evidenceItems,
        List<EvidenceGap> evidenceGaps,
        List<SourceReference> sourceReferences,
        string? userInstruction)
    {
        var builder = new StringBuilder();

        builder.AppendLine("# Veteran statement draft");
        builder.AppendLine();
        builder.AppendLine("Preparation support only. This draft is for review and editing before use.");
        builder.AppendLine();
        builder.AppendLine("## Condition");
        builder.AppendLine();
        builder.AppendLine($"This draft relates to {condition.ConditionName}.");
        builder.AppendLine($"Diagnosis status recorded in this workspace: {condition.DiagnosisStatus}.");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(userInstruction))
        {
            builder.AppendLine("## User instruction");
            builder.AppendLine();
            builder.AppendLine(userInstruction);
            builder.AppendLine();
        }

        builder.AppendLine("## Symptoms and current impact");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            GetConditionText(condition, "SymptomsSummary", "SymptomSummary", "Symptoms", "CurrentSymptoms", "ConditionSummary"),
            "No symptom summary has been added yet. Add plain-English symptoms before relying on this section.");

        builder.AppendLine();
        builder.AppendLine("## Treatment and medication");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            condition.TreatmentSummary,
            "No treatment summary has been added yet.");

        AppendParagraphOrPlaceholder(
            builder,
            condition.MedicationSummary,
            "No medication summary has been added yet.");

        builder.AppendLine();
        builder.AppendLine("## Daily life and functional impact");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            condition.FunctionalImpactSummary,
            "No functional impact summary has been added yet. Consider adding examples about daily routine, work, family, social life, domestic tasks, self-care, sleep, mobility, concentration or other relevant impacts.");

        builder.AppendLine();
        builder.AppendLine("## Accepted-condition or prior claim context");
        builder.AppendLine();

        if (acceptedHistory.Count == 0)
        {
            builder.AppendLine("No accepted-condition history has been added for this condition yet.");
        }
        else
        {
            foreach (var history in acceptedHistory)
            {
                builder.AppendLine($"- Previously accepted by DVA: {history.PreviouslyAcceptedByDva}; Original Act: {history.OriginalAct}; Previous compensation: {history.PreviousCompensationReceived}; Worsening claimed: {history.WorseningClaimed}; Notes: {history.WorseningSummary}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence already organised");
        builder.AppendLine();

        if (evidenceItems.Count == 0)
        {
            builder.AppendLine("No evidence items have been listed for this condition yet.");
        }
        else
        {
            foreach (var item in evidenceItems)
            {
                var uploadStatus = !string.IsNullOrWhiteSpace(item.StoragePath) && item.UploadedAt.HasValue
                    ? "uploaded"
                    : "listed, not uploaded";

                builder.AppendLine($"- {item.EvidenceType}: {item.EvidenceStatus}; File: {item.OriginalFileName ?? "not recorded"}; Provider: {item.ProviderName ?? "not recorded"}; Upload status: {uploadStatus}; Notes: {item.UserNotes ?? "none recorded"}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Follow-up items");
        builder.AppendLine();

        if (evidenceGaps.Count == 0)
        {
            builder.AppendLine("No active evidence gaps are currently recorded for this condition.");
        }
        else
        {
            foreach (var gap in evidenceGaps)
            {
                builder.AppendLine($"- {gap.GapType} ({gap.Severity}): {gap.PlainEnglishExplanation} Suggested next step: {gap.SuggestedNextStep}");
            }
        }

        AppendQuestionResponseSummary(builder, questionResponses);
        AppendSourceReferences(builder, sourceReferences);
        AppendSafetyFooter(builder);

        return builder.ToString();
    }

    private static string BuildWorseningSummaryDraft(
        ClaimWorkspace workspace,
        ClaimCondition condition,
        List<AcceptedConditionHistory> acceptedHistory,
        List<QuestionResponse> questionResponses,
        List<EvidenceItem> evidenceItems,
        List<EvidenceGap> evidenceGaps,
        List<SourceReference> sourceReferences,
        string? userInstruction)
    {
        var builder = new StringBuilder();

        builder.AppendLine("# Worsening summary draft");
        builder.AppendLine();
        builder.AppendLine("Preparation support only. This draft is for review and editing before use.");
        builder.AppendLine();
        builder.AppendLine("## Condition and previous context");
        builder.AppendLine();
        builder.AppendLine($"This draft relates to {condition.ConditionName}.");
        builder.AppendLine($"Diagnosis status recorded in this workspace: {condition.DiagnosisStatus}.");
        builder.AppendLine();

        if (!string.IsNullOrWhiteSpace(userInstruction))
        {
            builder.AppendLine("## User instruction");
            builder.AppendLine();
            builder.AppendLine(userInstruction);
            builder.AppendLine();
        }

        builder.AppendLine("## What has changed");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            condition.WorseningNotes,
            "No worsening notes have been added yet. Add details about what has changed, when it changed, and what is harder now before relying on this section.");

        builder.AppendLine();
        builder.AppendLine("## Current symptoms and functional impact");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            GetConditionText(condition, "SymptomsSummary", "SymptomSummary", "Symptoms", "CurrentSymptoms", "ConditionSummary"),
            "No current symptom summary has been added yet.");

        AppendParagraphOrPlaceholder(
            builder,
            condition.FunctionalImpactSummary,
            "No functional impact summary has been added yet.");

        builder.AppendLine();
        builder.AppendLine("## Treatment, medication or clinical changes");
        builder.AppendLine();
        AppendParagraphOrPlaceholder(
            builder,
            condition.TreatmentSummary,
            "No treatment change summary has been added yet.");

        AppendParagraphOrPlaceholder(
            builder,
            condition.MedicationSummary,
            "No medication change summary has been added yet.");

        builder.AppendLine();
        builder.AppendLine("## Prior DVA or accepted-condition information");
        builder.AppendLine();

        if (acceptedHistory.Count == 0)
        {
            builder.AppendLine("No accepted-condition history has been added for this condition yet.");
        }
        else
        {
            foreach (var history in acceptedHistory)
            {
                builder.AppendLine($"- Previously accepted by DVA: {history.PreviouslyAcceptedByDva}; Original Act: {history.OriginalAct}; Previous decision date: {history.PreviousDecisionDate}; Previous assessment date: {history.PreviousAssessmentDate}; Previous compensation: {history.PreviousCompensationReceived}; Worsening claimed: {history.WorseningClaimed}; Notes: {history.WorseningSummary}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence already organised");
        builder.AppendLine();

        if (evidenceItems.Count == 0)
        {
            builder.AppendLine("No evidence items have been listed for this condition yet.");
        }
        else
        {
            foreach (var item in evidenceItems)
            {
                var uploadStatus = !string.IsNullOrWhiteSpace(item.StoragePath) && item.UploadedAt.HasValue
                    ? "uploaded"
                    : "listed, not uploaded";

                builder.AppendLine($"- {item.EvidenceType}: {item.EvidenceStatus}; File: {item.OriginalFileName ?? "not recorded"}; Provider: {item.ProviderName ?? "not recorded"}; Upload status: {uploadStatus}; Notes: {item.UserNotes ?? "none recorded"}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("## Evidence gaps or follow-up questions");
        builder.AppendLine();

        if (evidenceGaps.Count == 0)
        {
            builder.AppendLine("No active evidence gaps are currently recorded for this condition.");
        }
        else
        {
            foreach (var gap in evidenceGaps)
            {
                builder.AppendLine($"- {gap.GapType} ({gap.Severity}): {gap.PlainEnglishExplanation} Suggested next step: {gap.SuggestedNextStep}");
            }
        }

        AppendQuestionResponseSummary(builder, questionResponses);
        AppendSourceReferences(builder, sourceReferences);
        AppendSafetyFooter(builder);

        return builder.ToString();
    }

    private static void AppendQuestionResponseSummary(StringBuilder builder, List<QuestionResponse> questionResponses)
    {
        builder.AppendLine();
        builder.AppendLine("## Other workspace answers");
        builder.AppendLine();

        if (questionResponses.Count == 0)
        {
            builder.AppendLine("No question responses have been recorded for this condition yet.");
            return;
        }

        foreach (var response in questionResponses)
        {
            builder.AppendLine($"- {response.QuestionGroup} / {response.QuestionKey}: {response.AnswerText}");
        }
    }

    private static void AppendSourceReferences(StringBuilder builder, List<SourceReference> sourceReferences)
    {
        builder.AppendLine();
        builder.AppendLine("## Source references");
        builder.AppendLine();

        if (sourceReferences.Count == 0)
        {
            builder.AppendLine("No approved source references were attached to this draft.");
            return;
        }

        foreach (var source in sourceReferences)
        {
            builder.AppendLine($"[{source.CitationMarker}] {source.CitationLabel} ({source.Category})");
        }
    }

    private static void AppendSafetyFooter(StringBuilder builder)
    {
        builder.AppendLine();
        builder.AppendLine("## Review note");
        builder.AppendLine();
        builder.AppendLine("This draft is preparation support only. Please check that it is accurate, complete and in your own words before using it. It is not legal advice, medical advice or a DVA decision.");
    }


    private static string? GetConditionText(ClaimCondition condition, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var property = typeof(ClaimCondition).GetProperty(propertyName);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(condition)?.ToString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }
    private static void AppendParagraphOrPlaceholder(StringBuilder builder, string? value, string placeholder)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine(placeholder);
            return;
        }

        builder.AppendLine(value.Trim());
    }

    private static async Task<List<KnowledgeBaseChunk>> RetrieveChunksAsync(string draftType, string query, int maxSources)
    {
        var chunks = await LoadKnowledgeBaseChunksAsync();

        return chunks
            .Where(IsEligibleForRetrieval)
            .Where(chunk =>
                SupportsDraftTask(chunk, draftType) ||
                string.Equals(chunk.Category, "SAFETY_GUARDRAIL", StringComparison.OrdinalIgnoreCase))
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = ScoreChunk(chunk, draftType, query)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Chunk.Category)
            .ThenBy(x => x.Chunk.ChunkKey)
            .Take(maxSources)
            .Select(x => x.Chunk)
            .ToList();
    }

    private static async Task<List<KnowledgeBaseChunk>> LoadKnowledgeBaseChunksAsync()
    {
        var candidatePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "KnowledgeBase", "seed-content", "knowledge-base-chunks.seed.jsonl"),
            Path.Combine(Directory.GetCurrentDirectory(), "KnowledgeBase", "seed-content", "knowledge-base-chunks.seed.jsonl"),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "knowledge-base", "seed-content", "knowledge-base-chunks.seed.jsonl"))
        };

        var seedPath = candidatePaths.FirstOrDefault(File.Exists);

        if (seedPath is null)
        {
            return new List<KnowledgeBaseChunk>();
        }

        var lines = await File.ReadAllLinesAsync(seedPath);
        var chunks = new List<KnowledgeBaseChunk>();

        foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            var chunk = JsonSerializer.Deserialize<KnowledgeBaseChunk>(line, JsonOptions);

            if (chunk is not null)
            {
                chunks.Add(chunk);
            }
        }

        return chunks;
    }

    private static bool IsEligibleForRetrieval(KnowledgeBaseChunk chunk)
    {
        return string.Equals(chunk.ApprovalStatus, "APPROVED", StringComparison.OrdinalIgnoreCase) &&
               chunk.IsActive &&
               string.Equals(chunk.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
               !string.IsNullOrWhiteSpace(chunk.ChunkKey) &&
               !string.IsNullOrWhiteSpace(chunk.SourceKey) &&
               !string.IsNullOrWhiteSpace(chunk.CitationLabel) &&
               !string.IsNullOrWhiteSpace(chunk.Content);
    }

    private static bool SupportsDraftTask(KnowledgeBaseChunk chunk, string draftType)
    {
        return chunk.RetrievalUse.Any(value =>
            string.Equals(value, "ALL", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, draftType, StringComparison.OrdinalIgnoreCase));
    }

    private static int ScoreChunk(KnowledgeBaseChunk chunk, string draftType, string query)
    {
        var score = 0;

        if (SupportsDraftTask(chunk, draftType))
        {
            score += 50;
        }

        if (string.Equals(chunk.Category, "SAFETY_GUARDRAIL", StringComparison.OrdinalIgnoreCase))
        {
            score += 40;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return score;
        }

        var terms = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.ToLowerInvariant())
            .Distinct()
            .ToList();

        var content = chunk.Content.ToLowerInvariant();
        var title = chunk.ChunkTitle.ToLowerInvariant();
        var category = chunk.Category.ToLowerInvariant();

        foreach (var term in terms)
        {
            if (title.Contains(term))
            {
                score += 8;
            }

            if (category.Contains(term))
            {
                score += 5;
            }

            if (content.Contains(term))
            {
                score += 3;
            }
        }

        return score;
    }

    private static string NormaliseDraftType(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "VETERAN_STATEMENT"
            : value.Trim().ToUpperInvariant();
    }

    private static bool IsSupportedDraftType(string value)
    {
        return GetSupportedDraftTypes().Contains(value);
    }

    private static string[] GetSupportedDraftTypes()
    {
        return new[]
        {
            "VETERAN_STATEMENT",
            "WORSENING_SUMMARY"
        };
    }

    private sealed class KnowledgeBaseChunk
    {
        public string ChunkKey { get; set; } = string.Empty;

        public string SourceKey { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string SourceType { get; set; } = string.Empty;

        public string CitationLabel { get; set; } = string.Empty;

        public string ChunkTitle { get; set; } = string.Empty;

        public string[] RetrievalUse { get; set; } = Array.Empty<string>();

        public string Content { get; set; } = string.Empty;

        public string SafetyNotes { get; set; } = string.Empty;

        public string ApprovalStatus { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string Status { get; set; } = string.Empty;
    }

    private sealed record SourceReference(
        string CitationMarker,
        string SourceKey,
        string CitationLabel,
        string Category,
        string SourceType,
        string ChunkKey,
        string ChunkTitle);
}

public sealed record GenerateAiDraftRequest(
    Guid? ConditionId,
    string? DraftType,
    string? Query,
    int? MaxSources,
    string? UserInstruction
);