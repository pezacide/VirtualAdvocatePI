using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class FunctionalImpactEntryEndpoints
{
    public static IEndpointRouteBuilder MapFunctionalImpactEntryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/functional-impact", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
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

            var rows = await db.FunctionalImpactEntries
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.ConditionId == conditionId && x.Status != "ARCHIVED")
                .OrderBy(x => x.ActivityDomain)
                .ThenByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(rows.Select(ToResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/functional-impact", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateFunctionalImpactEntryRequest input) =>
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

            if (string.IsNullOrWhiteSpace(input.GoodDayDescription) &&
                string.IsNullOrWhiteSpace(input.BadDayDescription))
            {
                return Results.BadRequest(new { error = "Describe a good day, a bad day, or both." });
            }

            var entry = new FunctionalImpactEntry
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = conditionId,
                ActivityDomain = NormaliseActivityDomain(input.ActivityDomain),
                GoodDayDescription = Trimmed(input.GoodDayDescription),
                BadDayDescription = Trimmed(input.BadDayDescription),
                BadDayFrequency = NormaliseFrequency(input.BadDayFrequency),
                AidsOrHelpUsed = Trimmed(input.AidsOrHelpUsed),
                Notes = Trimmed(input.Notes),
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.FunctionalImpactEntries.Add(entry);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "FUNCTIONAL_IMPACT_ENTRY_CREATED",
                $"Functional impact entry created. ConditionId={conditionId}; EntryId={entry.Id}; ActivityDomain={entry.ActivityDomain}");

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/functional-impact/{entry.Id}",
                ToResponse(entry));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/functional-impact/{entryId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid entryId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateFunctionalImpactEntryRequest input) =>
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

            var entry = await db.FunctionalImpactEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == entryId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (entry is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.ActivityDomain))
            {
                entry.ActivityDomain = NormaliseActivityDomain(input.ActivityDomain);
            }

            if (!string.IsNullOrWhiteSpace(input.BadDayFrequency))
            {
                entry.BadDayFrequency = NormaliseFrequency(input.BadDayFrequency);
            }

            entry.GoodDayDescription = input.GoodDayDescription is null ? entry.GoodDayDescription : Trimmed(input.GoodDayDescription);
            entry.BadDayDescription = input.BadDayDescription is null ? entry.BadDayDescription : Trimmed(input.BadDayDescription);
            entry.AidsOrHelpUsed = input.AidsOrHelpUsed is null ? entry.AidsOrHelpUsed : Trimmed(input.AidsOrHelpUsed);
            entry.Notes = input.Notes is null ? entry.Notes : Trimmed(input.Notes);
            entry.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "FUNCTIONAL_IMPACT_ENTRY_UPDATED",
                $"Functional impact entry updated. ConditionId={conditionId}; EntryId={entry.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToResponse(entry));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/functional-impact/{entryId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid entryId,
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

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var entry = await db.FunctionalImpactEntries
                .FirstOrDefaultAsync(x =>
                    x.Id == entryId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (entry is null)
            {
                return Results.NotFound();
            }

            entry.Status = "ARCHIVED";
            entry.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "FUNCTIONAL_IMPACT_ENTRY_ARCHIVED",
                $"Functional impact entry archived. ConditionId={conditionId}; EntryId={entry.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new { id = entry.Id, status = entry.Status, archived = true });
        });

        return app;
    }

    internal static object ToResponse(FunctionalImpactEntry entry)
    {
        return new
        {
            id = entry.Id,
            claimWorkspaceId = entry.ClaimWorkspaceId,
            conditionId = entry.ConditionId,
            activityDomain = entry.ActivityDomain,
            goodDayDescription = entry.GoodDayDescription,
            badDayDescription = entry.BadDayDescription,
            badDayFrequency = entry.BadDayFrequency,
            aidsOrHelpUsed = entry.AidsOrHelpUsed,
            notes = entry.Notes,
            status = entry.Status,
            createdAt = entry.CreatedAt,
            updatedAt = entry.UpdatedAt
        };
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string NormaliseActivityDomain(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "SELF_CARE" or "MOBILITY" or "LIFTING_CARRYING" or "HOUSEHOLD" or "WORK"
                or "SLEEP" or "SOCIAL" or "DRIVING" or "RECREATION" or "OTHER" => normalised,
            _ => "OTHER"
        };
    }

    private static string NormaliseFrequency(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "DAILY" or "MOST_DAYS" or "WEEKLY" or "MONTHLY" or "OCCASIONAL" or "FLARE_UPS_ONLY" or "UNSURE" => normalised,
            _ => "UNSURE"
        };
    }
}

public sealed record CreateFunctionalImpactEntryRequest(
    string? ActivityDomain,
    string? GoodDayDescription,
    string? BadDayDescription,
    string? BadDayFrequency,
    string? AidsOrHelpUsed,
    string? Notes
);

public sealed record UpdateFunctionalImpactEntryRequest(
    string? ActivityDomain,
    string? GoodDayDescription,
    string? BadDayDescription,
    string? BadDayFrequency,
    string? AidsOrHelpUsed,
    string? Notes
);
