using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class LoadReferenceItemSeedEndpoints
{
    public static IEndpointRouteBuilder MapLoadReferenceItemSeedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/reference/load-reference-items/seed", async (
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

            if (!adminAccessService.GetAccess(user).IsAdmin)
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var seedPath = ResolveSeedPath();

            if (seedPath is null)
            {
                return Results.NotFound(new
                {
                    error = "Load reference items seed file was not found.",
                    expectedRelativePath = "KnowledgeBase/reference/load-reference-items.seed.json"
                });
            }

            var json = await File.ReadAllTextAsync(seedPath, cancellationToken);

            var seedFile = JsonSerializer.Deserialize<LoadReferenceItemSeedFile>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (seedFile?.Items is null || seedFile.Items.Count == 0)
            {
                return Results.BadRequest(new { error = "Seed file did not contain any items.", seedPath });
            }

            var existingNames = await db.LoadReferenceItems
                .Select(x => x.ItemName)
                .ToListAsync(cancellationToken);

            var existingNameSet = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var inserted = 0;
            var skipped = 0;
            var now = DateTimeOffset.UtcNow;
            var insertedNames = new List<string>();

            foreach (var seedItem in seedFile.Items)
            {
                if (string.IsNullOrWhiteSpace(seedItem.ItemName))
                {
                    continue;
                }

                var itemName = seedItem.ItemName.Trim();

                if (existingNameSet.Contains(itemName))
                {
                    skipped++;
                    continue;
                }

                db.LoadReferenceItems.Add(new LoadReferenceItem
                {
                    Id = Guid.NewGuid(),
                    ItemName = itemName,
                    Category = string.IsNullOrWhiteSpace(seedItem.Category) ? "OTHER" : seedItem.Category.Trim().ToUpperInvariant(),
                    TypicalWeightKg = seedItem.TypicalWeightKg,
                    WeightRangeKg = NullIfWhiteSpace(seedItem.WeightRangeKg),
                    ServiceContext = NullIfWhiteSpace(seedItem.ServiceContext),
                    Notes = NullIfWhiteSpace(seedItem.Notes),
                    SourceLabel = string.IsNullOrWhiteSpace(seedItem.SourceLabel)
                        ? (seedFile.SourceLabel ?? "Virtual Advocate PI starter reference set")
                        : seedItem.SourceLabel.Trim(),
                    Status = "ACTIVE",
                    CreatedAt = now,
                    UpdatedAt = now
                });

                existingNameSet.Add(itemName);
                inserted++;
                insertedNames.Add(itemName);
            }

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                seeded = true,
                requestedBy = user.Email,
                seedPath,
                totalItemsInSeedFile = seedFile.Items.Count,
                inserted,
                skippedExisting = skipped,
                insertedNames,
                message = "Load reference items seed process completed."
            });
        });

        return app;
    }

    private static string? ResolveSeedPath()
    {
        var relativePath = Path.Combine("KnowledgeBase", "reference", "load-reference-items.seed.json");

        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath)
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class LoadReferenceItemSeedFile
{
    public string? SchemaVersion { get; set; }
    public string? Purpose { get; set; }
    public string? SourceLabel { get; set; }
    public List<LoadReferenceItemSeedEntry> Items { get; set; } = new();
}

public sealed class LoadReferenceItemSeedEntry
{
    public string? ItemName { get; set; }
    public string? Category { get; set; }
    public decimal? TypicalWeightKg { get; set; }
    public string? WeightRangeKg { get; set; }
    public string? ServiceContext { get; set; }
    public string? Notes { get; set; }
    public string? SourceLabel { get; set; }
}
