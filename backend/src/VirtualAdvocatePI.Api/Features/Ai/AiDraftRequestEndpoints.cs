using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Ai;

public static class AiDraftRequestEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapAiDraftRequestEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/ai-drafts/request", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            AiDraftRequestInput input) =>
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

            var draftTaskType = NormaliseDraftTaskType(input.DraftTaskType);

            if (!IsValidDraftTaskType(draftTaskType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid draft task type.",
                    allowedValues = GetAllowedDraftTaskTypes()
                });
            }

            var aiDraftType = MapDraftTaskTypeToAiDraftType(draftTaskType);
            var query = input.Query?.Trim() ?? string.Empty;
            var userInstruction = input.UserInstruction?.Trim();

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == workspaceId && x.Status != "ARCHIVED");

            if (workspace is null)
            {
                return Results.NotFound();
            }

            var condition = input.ConditionId.HasValue
                ? await db.ClaimConditions
                    .FirstOrDefaultAsync(x =>
                        x.Id == input.ConditionId.Value &&
                        x.ClaimWorkspaceId == workspaceId &&
                        x.Status != "ARCHIVED")
                : null;

            var activeConditions = await db.ClaimConditions
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderBy(x => x.ConditionName)
                .ToListAsync();

            var acceptedHistory = await db.AcceptedConditionHistories
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    (!input.ConditionId.HasValue || x.ConditionId == input.ConditionId.Value))
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            var questionResponses = await db.QuestionResponses
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    (!input.ConditionId.HasValue || x.ConditionId == input.ConditionId.Value))
                .OrderBy(x => x.QuestionGroup)
                .ThenBy(x => x.QuestionKey)
                .ToListAsync();

            var evidenceItems = await db.EvidenceItems
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    (!input.ConditionId.HasValue || x.ConditionId == input.ConditionId.Value))
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            var evidenceGaps = await db.EvidenceGaps
                .Where(x =>
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED" &&
                    (!input.ConditionId.HasValue || x.ConditionId == input.ConditionId.Value))
                .OrderByDescending(x => x.Severity)
                .ThenBy(x => x.GapType)
                .ToListAsync();

            var retrievedChunks = await RetrieveChunksAsync(draftTaskType, query, Math.Clamp(input.MaxSources ?? 8, 1, 12));
            var sharedSafetyPrompt = await LoadPromptTemplateAsync("shared", "safety-guardrails.prompt.md");
            var taskPrompt = await LoadPromptForDraftTaskAsync(draftTaskType);

            var workspaceDataSummary = BuildWorkspaceDataSummary(
                workspace,
                condition,
                activeConditions,
                acceptedHistory,
                questionResponses,
                evidenceItems,
                evidenceGaps);

            var sourceReferences = retrievedChunks
                .Select((chunk, index) => new
                {
                    citationMarker = $"S{index + 1}",
                    sourceKey = chunk.SourceKey,
                    citationLabel = chunk.CitationLabel,
                    category = chunk.Category,
                    sourceType = chunk.SourceType,
                    chunkKey = chunk.ChunkKey,
                    chunkTitle = chunk.ChunkTitle,
                    safetyNotes = chunk.SafetyNotes
                })
                .ToList();

            var promptPackage = BuildPromptPackage(
                draftTaskType,
                aiDraftType,
                query,
                userInstruction,
                sharedSafetyPrompt,
                taskPrompt,
                workspaceDataSummary,
                sourceReferences,
                retrievedChunks);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_DRAFT_REQUESTED",
                $"AI draft request package created. DraftTaskType={draftTaskType}; AiDraftType={aiDraftType}; ConditionId={input.ConditionId}; SourceCount={sourceReferences.Count}; QueryLength={query.Length}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                workspaceId,
                conditionId = input.ConditionId,
                draftTaskType,
                aiDraftType,
                promptVersion = "rag-draft-request-v1",
                query,
                userInstruction,
                sourceReferenceCount = sourceReferences.Count,
                sourceReferences,
                workspaceData = new
                {
                    workspace = new
                    {
                        workspace.Id,
                        workspace.WorkspaceTitle,
                        workspace.ClaimFramework,
                        workspace.ClaimScenario,
                        workspace.GeneratedPackStatus
                    },
                    selectedCondition = condition is null
                        ? null
                        : new
                        {
                            condition.Id,
                            condition.ConditionName,
                            condition.DiagnosisStatus,
                            condition.IsPrimaryCondition
                        },
                    activeConditionCount = activeConditions.Count,
                    acceptedHistoryCount = acceptedHistory.Count,
                    questionResponseCount = questionResponses.Count,
                    evidenceItemCount = evidenceItems.Count,
                    evidenceGapCount = evidenceGaps.Count
                },
                promptPackage,
                safety = new
                {
                    preparationSupportOnly = true,
                    requiresUserReview = true,
                    legalAdvice = false,
                    medicalAdvice = false,
                    dvaDecision = false,
                    diagnosis = false,
                    impairmentCalculation = false,
                    compensationEstimate = false,
                    outcomeGuarantee = false,
                    aiGenerationEnabled = false
                }
            });
        });

        return app;
    }

    private static string BuildWorkspaceDataSummary(
        dynamic workspace,
        dynamic? condition,
        IEnumerable<dynamic> activeConditions,
        IEnumerable<dynamic> acceptedHistory,
        IEnumerable<dynamic> questionResponses,
        IEnumerable<dynamic> evidenceItems,
        IEnumerable<dynamic> evidenceGaps)
    {
        var builder = new StringBuilder();

        builder.AppendLine("## Active workspace data");
        builder.AppendLine();
        builder.AppendLine($"Workspace title: {workspace.WorkspaceTitle}");
        builder.AppendLine($"Claim framework: {workspace.ClaimFramework}");
        builder.AppendLine($"Claim scenario: {workspace.ClaimScenario}");
        builder.AppendLine($"Generated pack status: {workspace.GeneratedPackStatus}");
        builder.AppendLine();

        if (condition is not null)
        {
            builder.AppendLine("## Selected condition");
            builder.AppendLine();
            builder.AppendLine($"Condition name: {condition.ConditionName}");
            builder.AppendLine($"Diagnosis status: {condition.DiagnosisStatus}");
            builder.AppendLine($"Primary condition: {condition.IsPrimaryCondition}");
            builder.AppendLine($"Symptoms summary: {condition.SymptomsSummary}");
            builder.AppendLine($"Treatment summary: {condition.TreatmentSummary}");
            builder.AppendLine($"Medication summary: {condition.MedicationSummary}");
            builder.AppendLine($"Functional impact summary: {condition.FunctionalImpactSummary}");
            builder.AppendLine($"Worsening notes: {condition.WorseningNotes}");
            builder.AppendLine();
        }
        else
        {
            builder.AppendLine("## Active conditions");
            builder.AppendLine();

            foreach (var item in activeConditions)
            {
                builder.AppendLine($"- {item.ConditionName} | Diagnosis status: {item.DiagnosisStatus} | Primary: {item.IsPrimaryCondition}");
            }

            builder.AppendLine();
        }

        builder.AppendLine("## Accepted-condition history");
        builder.AppendLine();

        foreach (var item in acceptedHistory)
        {
            builder.AppendLine($"- ConditionId={item.ConditionId}; Previously accepted by DVA={item.PreviouslyAcceptedByDva}; Original Act={item.OriginalAct}; Previous compensation={item.PreviousCompensationReceived}; Worsening claimed={item.WorseningClaimed}; Summary={item.WorseningSummary}");
        }

        builder.AppendLine();

        builder.AppendLine("## Question responses");
        builder.AppendLine();

        foreach (var item in questionResponses)
        {
            builder.AppendLine($"- {item.QuestionGroup} / {item.QuestionKey}: {item.AnswerText}");
        }

        builder.AppendLine();

        builder.AppendLine("## Evidence items");
        builder.AppendLine();

        foreach (var item in evidenceItems)
        {
            builder.AppendLine($"- Type={item.EvidenceType}; Status={item.EvidenceStatus}; File={item.OriginalFileName}; Provider={item.ProviderName}; DocumentDate={item.DocumentDate}; UploadedAt={item.UploadedAt}; Notes={item.UserNotes}");
        }

        builder.AppendLine();

        builder.AppendLine("## Evidence gaps");
        builder.AppendLine();

        foreach (var item in evidenceGaps)
        {
            builder.AppendLine($"- GapType={item.GapType}; GapStatus={item.GapStatus}; Severity={item.Severity}; Explanation={item.PlainEnglishExplanation}; SuggestedNextStep={item.SuggestedNextStep}");
        }

        return builder.ToString();
    }

    private static string BuildPromptPackage(
        string draftTaskType,
        string aiDraftType,
        string query,
        string? userInstruction,
        string sharedSafetyPrompt,
        string taskPrompt,
        string workspaceDataSummary,
        IEnumerable<object> sourceReferences,
        IEnumerable<KnowledgeBaseChunk> retrievedChunks)
    {
        var builder = new StringBuilder();

        builder.AppendLine("# AI draft request package");
        builder.AppendLine();
        builder.AppendLine($"Draft task type: {draftTaskType}");
        builder.AppendLine($"AI draft type: {aiDraftType}");
        builder.AppendLine($"Query: {query}");
        builder.AppendLine($"User instruction: {userInstruction}");
        builder.AppendLine();
        builder.AppendLine("Important: This package is for future AI draft generation. It is not a completed draft.");
        builder.AppendLine();

        builder.AppendLine(sharedSafetyPrompt);
        builder.AppendLine();

        builder.AppendLine(taskPrompt);
        builder.AppendLine();

        builder.AppendLine(workspaceDataSummary);
        builder.AppendLine();

        builder.AppendLine("## Approved source chunks");
        builder.AppendLine();

        var indexedChunks = retrievedChunks.Select((chunk, index) => new
        {
            Marker = $"S{index + 1}",
            Chunk = chunk
        });

        foreach (var item in indexedChunks)
        {
            builder.AppendLine($"[{item.Marker}] {item.Chunk.ChunkTitle}");
            builder.AppendLine($"Source: {item.Chunk.CitationLabel}");
            builder.AppendLine($"Content: {item.Chunk.Content}");
            builder.AppendLine($"Safety notes: {item.Chunk.SafetyNotes}");
            builder.AppendLine();
        }

        builder.AppendLine("## Required output safety");
        builder.AppendLine();
        builder.AppendLine("The generated draft must be preparation support only, must be reviewable by the user, and must not provide legal advice, medical advice, DVA decisions, diagnosis, impairment calculations, compensation estimates or outcome guarantees.");

        return builder.ToString();
    }

    private static async Task<List<KnowledgeBaseChunk>> RetrieveChunksAsync(string draftTaskType, string query, int maxSources)
    {
        var chunks = await LoadKnowledgeBaseChunksAsync();

        return chunks
            .Where(IsEligibleForRetrieval)
            .Where(chunk =>
                SupportsDraftTask(chunk, draftTaskType) ||
                string.Equals(chunk.Category, "SAFETY_GUARDRAIL", StringComparison.OrdinalIgnoreCase))
            .Select(chunk => new
            {
                Chunk = chunk,
                Score = ScoreChunk(chunk, draftTaskType, query)
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

    private static async Task<string> LoadPromptForDraftTaskAsync(string draftTaskType)
    {
        var relativePath = draftTaskType switch
        {
            "VETERAN_STATEMENT" => Path.Combine("veteran-statement", "veteran-statement.prompt.md"),
            "WORSENING_SUMMARY" => Path.Combine("worsening-summary", "worsening-summary.prompt.md"),
            "DOCTOR_QUESTIONS" => Path.Combine("doctor-questions", "doctor-questions.prompt.md"),
            "EVIDENCE_GAP_SUMMARY" => Path.Combine("evidence-gap-summary", "evidence-gap-summary.prompt.md"),
            "DOCTOR_REQUEST_LETTER" => Path.Combine("doctor-request-letter", "doctor-request-letter.prompt.md"),
            "CLAIM_PACK_COVER_NOTE" => Path.Combine("doctor-request-letter", "doctor-request-letter.prompt.md"),
            _ => Path.Combine("veteran-statement", "veteran-statement.prompt.md")
        };

        return await LoadPromptTemplateAsync(relativePath);
    }

    private static async Task<string> LoadPromptTemplateAsync(params string[] pathParts)
    {
        var relativePath = Path.Combine(pathParts);

        var candidatePaths = new[]
        {
            Path.Combine(new[] { AppContext.BaseDirectory, "KnowledgeBase", "prompt-templates" }.Concat(pathParts).ToArray()),
            Path.Combine(new[] { Directory.GetCurrentDirectory(), "KnowledgeBase", "prompt-templates" }.Concat(pathParts).ToArray()),
            Path.GetFullPath(Path.Combine(new[] { Directory.GetCurrentDirectory(), "..", "..", "..", "knowledge-base", "prompt-templates" }.Concat(pathParts).ToArray()))
        };

        var path = candidatePaths.FirstOrDefault(File.Exists);

        if (path is null)
        {
            return $"Prompt template not found: {relativePath}";
        }

        return await File.ReadAllTextAsync(path);
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

    private static bool SupportsDraftTask(KnowledgeBaseChunk chunk, string draftTaskType)
    {
        return chunk.RetrievalUse.Any(value =>
            string.Equals(value, "ALL", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, draftTaskType, StringComparison.OrdinalIgnoreCase));
    }

    private static int ScoreChunk(KnowledgeBaseChunk chunk, string draftTaskType, string query)
    {
        var score = 0;

        if (SupportsDraftTask(chunk, draftTaskType))
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

    private static string NormaliseDraftTaskType(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "VETERAN_STATEMENT"
            : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidDraftTaskType(string value)
    {
        return GetAllowedDraftTaskTypes().Contains(value);
    }

    private static string[] GetAllowedDraftTaskTypes()
    {
        return new[]
        {
            "VETERAN_STATEMENT",
            "WORSENING_SUMMARY",
            "EVIDENCE_GAP_SUMMARY",
            "DOCTOR_QUESTIONS",
            "DOCTOR_REQUEST_LETTER",
            "CLAIM_PACK_COVER_NOTE"
        };
    }

    private static string MapDraftTaskTypeToAiDraftType(string draftTaskType)
    {
        return draftTaskType switch
        {
            "DOCTOR_QUESTIONS" => "DOCTOR_APPOINTMENT_QUESTIONS",
            _ => draftTaskType
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
}

public sealed record AiDraftRequestInput(
    Guid? ConditionId,
    string? DraftTaskType,
    string? Query,
    int? MaxSources,
    string? UserInstruction
);