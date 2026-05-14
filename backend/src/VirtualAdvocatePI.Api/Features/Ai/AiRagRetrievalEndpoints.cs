using System.Text.Json;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Ai;

public static class AiRagRetrievalEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IEndpointRouteBuilder MapAiRagRetrievalEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/ai-rag/retrieve", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            RagRetrievalRequest input) =>
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

            var maxResults = Math.Clamp(input.MaxResults ?? 8, 1, 12);
            var query = input.Query?.Trim() ?? string.Empty;

            var chunks = await LoadKnowledgeBaseChunksAsync();

            var eligibleChunks = chunks
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
                .Take(maxResults)
                .Select(x => x.Chunk)
                .ToList();

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "AI_RAG_RETRIEVAL_REQUESTED",
                $"AI/RAG retrieval requested. DraftTaskType={draftTaskType}; ConditionId={input.ConditionId}; QueryLength={query.Length}; ReturnedChunks={eligibleChunks.Count}");

            await db.SaveChangesAsync();

            var sourceReferences = eligibleChunks
                .GroupBy(x => x.SourceKey)
                .Select(group =>
                {
                    var first = group.First();

                    return new
                    {
                        sourceKey = first.SourceKey,
                        citationLabel = first.CitationLabel,
                        category = first.Category,
                        sourceType = first.SourceType,
                        chunkCount = group.Count()
                    };
                })
                .ToList();

            return Results.Ok(new
            {
                workspaceId,
                conditionId = input.ConditionId,
                draftTaskType,
                query,
                maxResults,
                returnedChunkCount = eligibleChunks.Count,
                sourceReferences,
                chunks = eligibleChunks.Select(chunk => new
                {
                    chunkKey = chunk.ChunkKey,
                    sourceKey = chunk.SourceKey,
                    category = chunk.Category,
                    sourceType = chunk.SourceType,
                    citationLabel = chunk.CitationLabel,
                    chunkTitle = chunk.ChunkTitle,
                    retrievalUse = chunk.RetrievalUse,
                    content = chunk.Content,
                    safetyNotes = chunk.SafetyNotes
                }).ToList(),
                safety = new
                {
                    preparationSupportOnly = true,
                    legalAdvice = false,
                    medicalAdvice = false,
                    dvaDecision = false,
                    impairmentCalculation = false,
                    compensationEstimate = false,
                    outcomeGuarantee = false
                }
            });
        });

        return app;
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
            "CLAIM_PACK_COVER_NOTE",
            "GENERATED_DOCUMENT",
            "ALL"
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

public sealed record RagRetrievalRequest(
    Guid? ConditionId,
    string? DraftTaskType,
    string? Query,
    int? MaxResults
);