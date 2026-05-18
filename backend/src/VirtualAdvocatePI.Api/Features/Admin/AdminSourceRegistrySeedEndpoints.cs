using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminSourceRegistrySeedEndpoints
{
    public static IEndpointRouteBuilder MapAdminSourceRegistrySeedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/source-registry/seed-approved", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            CancellationToken cancellationToken) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var access = adminAccessService.GetAccess(user);

            if (!access.IsAdmin)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var seedPath = ResolveSeedPath();

            if (seedPath is null)
            {
                return Results.NotFound(new
                {
                    error = "Approved source registry seed file was not found.",
                    expectedRelativePath = "KnowledgeBase/source-registry/approved-source-registry.loaded.seed.json",
                    checkedBaseDirectory = AppContext.BaseDirectory,
                    checkedCurrentDirectory = Directory.GetCurrentDirectory()
                });
            }

            var json = await File.ReadAllTextAsync(seedPath, cancellationToken);

            var seedFile = JsonSerializer.Deserialize<ApprovedSourceRegistrySeedFile>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (seedFile?.Entries is null || seedFile.Entries.Count == 0)
            {
                return Results.BadRequest(new
                {
                    error = "Seed file did not contain any entries.",
                    seedPath
                });
            }

            var existingKeys = await db.AiSourceRegistryEntries
                .Select(x => x.SourceKey)
                .ToListAsync(cancellationToken);

            var existingKeySet = existingKeys.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inserted = 0;
            var skipped = 0;
            var skippedMissingKey = 0;
            var now = DateTimeOffset.UtcNow;
            var insertedKeys = new List<string>();
            var skippedKeys = new List<string>();

            foreach (var seedEntry in seedFile.Entries)
            {
                if (string.IsNullOrWhiteSpace(seedEntry.SourceKey))
                {
                    skippedMissingKey++;
                    continue;
                }

                var sourceKey = seedEntry.SourceKey.Trim();

                if (existingKeySet.Contains(sourceKey))
                {
                    skipped++;
                    skippedKeys.Add(sourceKey);
                    continue;
                }

                var entry = new AiSourceRegistryEntry
                {
                    Id = Guid.NewGuid(),
                    SourceKey = sourceKey,
                    Title = RequiredOrFallback(seedEntry.Title, sourceKey),
                    Category = RequiredOrFallback(seedEntry.Category, "GENERAL"),
                    SourceType = RequiredOrFallback(seedEntry.SourceType, "REFERENCE"),
                    Jurisdiction = RequiredOrFallback(seedEntry.Jurisdiction, "AUSTRALIA_DVA"),
                    SourceVersion = NullIfWhiteSpace(seedEntry.SourceVersion),
                    CitationLabel = RequiredOrFallback(seedEntry.CitationLabel, seedEntry.Title ?? sourceKey),
                    SourceUrl = NullIfWhiteSpace(seedEntry.SourceUrl),
                    StoragePath = NullIfWhiteSpace(seedEntry.StoragePath),
                    ContentHash = NullIfWhiteSpace(seedEntry.ContentHash),
                    ApprovalStatus = RequiredOrFallback(seedEntry.ApprovalStatus, "APPROVED").ToUpperInvariant(),
                    ApprovedBy = NullIfWhiteSpace(seedEntry.ApprovedBy) ?? user.Email,
                    ReviewNotes = NullIfWhiteSpace(seedEntry.ReviewNotes),
                    IsActive = seedEntry.IsActive ?? true,
                    Status = RequiredOrFallback(seedEntry.Status, "ACTIVE").ToUpperInvariant(),
                    CreatedAt = now,
                    UpdatedAt = now
                };

                db.AiSourceRegistryEntries.Add(entry);

                existingKeySet.Add(sourceKey);
                inserted++;
                insertedKeys.Add(sourceKey);
            }

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                seeded = true,
                requestedBy = user.Email,
                schemaVersion = seedFile.SchemaVersion,
                seedPath,
                totalEntriesInSeedFile = seedFile.Entries.Count,
                inserted,
                skippedExisting = skipped,
                skippedMissingKey,
                insertedKeys,
                skippedKeys,
                message = "Approved source registry seed process completed."
            });
        });

        return app;
    }

    private static string? ResolveSeedPath()
    {
        var relativePath = Path.Combine(
            "KnowledgeBase",
            "source-registry",
            "approved-source-registry.loaded.seed.json");

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string RequiredOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}

public sealed class ApprovedSourceRegistrySeedFile
{
    public string? SchemaVersion { get; set; }
    public string? Purpose { get; set; }
    public string? SafetyRule { get; set; }
    public List<ApprovedSourceRegistrySeedEntry> Entries { get; set; } = new();
}

public sealed class ApprovedSourceRegistrySeedEntry
{
    public string? SourceKey { get; set; }
    public string? Title { get; set; }
    public string? Category { get; set; }
    public string? SourceType { get; set; }
    public string? Jurisdiction { get; set; }
    public string? SourceVersion { get; set; }
    public string? CitationLabel { get; set; }
    public string? SourceUrl { get; set; }
    public string? StoragePath { get; set; }
    public string? ContentHash { get; set; }
    public string? ApprovalStatus { get; set; }
    public bool? IsActive { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ReviewNotes { get; set; }
    public string? Status { get; set; }
}