using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Admin;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

/// <summary>
/// Seeds the Functional and Load Evidence admin templates and prompt/disclaimer
/// versions. Entries are always created as DRAFT (never auto-approved) and
/// existing keys are left untouched, so re-running is safe.
/// </summary>
public static class FunctionalLoadEvidenceAdminSeedEndpoints
{
    public static IEndpointRouteBuilder MapFunctionalLoadEvidenceAdminSeedEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/functional-load-evidence/seed", async (
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
                    error = "Functional and load evidence admin seed file was not found.",
                    expectedRelativePath = "KnowledgeBase/admin/functional-load-evidence.seed.json"
                });
            }

            var seed = JsonSerializer.Deserialize<AdminSeedFile>(
                await File.ReadAllTextAsync(seedPath, cancellationToken),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (seed is null || (seed.Templates.Count == 0 && seed.Versions.Count == 0))
            {
                return Results.BadRequest(new { error = "Seed file did not contain any entries." });
            }

            var existingTemplateKeys = (await db.AdminTemplateRegistryEntries
                .Select(x => x.TemplateKey)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var existingVersionKeys = (await db.AdminPromptDisclaimerVersionEntries
                .Select(x => x.VersionKey)
                .ToListAsync(cancellationToken))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var now = DateTimeOffset.UtcNow;
            var templatesInserted = 0;
            var versionsInserted = 0;

            foreach (var t in seed.Templates.Where(t => !string.IsNullOrWhiteSpace(t.TemplateKey)))
            {
                if (!existingTemplateKeys.Add(t.TemplateKey!.Trim()))
                {
                    continue;
                }

                db.AdminTemplateRegistryEntries.Add(new AdminTemplateRegistryEntry
                {
                    Id = Guid.NewGuid(),
                    TemplateKey = t.TemplateKey.Trim(),
                    TemplateType = t.TemplateType?.Trim().ToUpperInvariant() ?? "QUESTION",
                    Title = t.Title ?? t.TemplateKey,
                    Description = t.Description ?? string.Empty,
                    Category = t.Category ?? "GENERAL",
                    TemplateVersion = "v1",
                    TemplateBody = t.TemplateBody ?? string.Empty,
                    OutputFormat = t.OutputFormat ?? "TEXT",
                    ApprovalStatus = "DRAFT",
                    IsActive = true,
                    Status = "ACTIVE",
                    CreatedAt = now,
                    UpdatedAt = now
                });
                templatesInserted++;
            }

            foreach (var v in seed.Versions.Where(v => !string.IsNullOrWhiteSpace(v.VersionKey)))
            {
                if (!existingVersionKeys.Add(v.VersionKey!.Trim()))
                {
                    continue;
                }

                db.AdminPromptDisclaimerVersionEntries.Add(new AdminPromptDisclaimerVersionEntry
                {
                    Id = Guid.NewGuid(),
                    VersionKey = v.VersionKey.Trim(),
                    VersionType = v.VersionType?.Trim().ToUpperInvariant() ?? "PROMPT",
                    Title = v.Title ?? v.VersionKey,
                    Description = v.Description ?? string.Empty,
                    Category = v.Category ?? "GENERAL",
                    VersionLabel = v.VersionLabel ?? "v1",
                    AppliesTo = v.AppliesTo ?? "GENERAL",
                    Content = v.Content ?? string.Empty,
                    ApprovalStatus = "DRAFT",
                    IsActive = true,
                    Status = "ACTIVE",
                    CreatedAt = now,
                    UpdatedAt = now
                });
                versionsInserted++;
            }

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(new
            {
                seeded = true,
                requestedBy = user.Email,
                templatesInserted,
                versionsInserted,
                message = "Entries are seeded as DRAFT and must be reviewed and approved before use."
            });
        });

        return app;
    }

    private static string? ResolveSeedPath()
    {
        var relativePath = Path.Combine("KnowledgeBase", "admin", "functional-load-evidence.seed.json");

        return new[]
        {
            Path.Combine(AppContext.BaseDirectory, relativePath),
            Path.Combine(Directory.GetCurrentDirectory(), relativePath)
        }.FirstOrDefault(File.Exists);
    }

    private sealed class AdminSeedFile
    {
        public List<TemplateSeed> Templates { get; set; } = new();
        public List<VersionSeed> Versions { get; set; } = new();
    }

    private sealed class TemplateSeed
    {
        public string? TemplateKey { get; set; }
        public string? TemplateType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? TemplateBody { get; set; }
        public string? OutputFormat { get; set; }
    }

    private sealed class VersionSeed
    {
        public string? VersionKey { get; set; }
        public string? VersionType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? VersionLabel { get; set; }
        public string? AppliesTo { get; set; }
        public string? Content { get; set; }
    }
}
