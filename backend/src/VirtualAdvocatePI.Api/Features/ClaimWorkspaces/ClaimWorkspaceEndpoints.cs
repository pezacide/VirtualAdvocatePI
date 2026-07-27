using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class ClaimWorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapClaimWorkspaceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var workspaceRows = await db.ClaimWorkspaces
                .Where(x => x.UserId == user.Id && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(workspaceRows.Select(ToClaimWorkspaceResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateClaimWorkspaceRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var claimScenario = NormaliseClaimScenario(input.ClaimScenario);

            if (!IsValidClaimScenario(claimScenario))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid claim scenario.",
                    allowedValues = GetAllowedClaimScenarios()
                });
            }

            var workspace = new ClaimWorkspace
            {
                UserId = user.Id,
                ClaimFramework = "IMPROVED_MRCA_POST_2026",
                ClaimScenario = claimScenario,
                WorkspaceTitle = string.IsNullOrWhiteSpace(input.WorkspaceTitle)
                    ? "Post-2026 PI Claim Starter Pack"
                    : input.WorkspaceTitle.Trim(),
                Status = "IN_PROGRESS",
                GeneratedPackStatus = "NOT_GENERATED",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                LastOpenedAt = DateTimeOffset.UtcNow
            };

            db.ClaimWorkspaces.Add(workspace);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspace.Id,
                "CLAIM_WORKSPACE_CREATED",
                $"Claim workspace created. Scenario={claimScenario}; WorkspaceId={workspace.Id}");

            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/claim-workspaces/{workspace.Id}", ToClaimWorkspaceResponse(workspace));
        });

        app.MapGet("/api/v1/claim-workspaces/{id:guid}", async (
            Guid id,
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

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
            {
                return Results.NotFound();
            }

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

            if (workspace is null)
            {
                return Results.NotFound();
            }

            workspace.LastOpenedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync();

            return Results.Ok(ToClaimWorkspaceResponse(workspace));
        });

        app.MapPatch("/api/v1/claim-workspaces/{id:guid}", async (
            Guid id,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateClaimWorkspaceRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
            {
                return Results.NotFound();
            }

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

            if (workspace is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.WorkspaceTitle))
            {
                workspace.WorkspaceTitle = input.WorkspaceTitle.Trim();
            }

            if (!string.IsNullOrWhiteSpace(input.ClaimScenario))
            {
                var claimScenario = NormaliseClaimScenario(input.ClaimScenario);

                if (!IsValidClaimScenario(claimScenario))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid claim scenario.",
                        allowedValues = GetAllowedClaimScenarios()
                    });
                }

                workspace.ClaimScenario = claimScenario;
            }

            if (!string.IsNullOrWhiteSpace(input.Status))
            {
                var status = input.Status.Trim().ToUpperInvariant();

                if (!IsValidWorkspaceStatus(status))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid workspace status.",
                        allowedValues = GetAllowedWorkspaceStatuses()
                    });
                }

                workspace.Status = status;
            }

            workspace.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspace.Id,
                "CLAIM_WORKSPACE_UPDATED",
                $"Claim workspace updated. Status={workspace.Status}; WorkspaceId={workspace.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToClaimWorkspaceResponse(workspace));
        });

        app.MapDelete("/api/v1/claim-workspaces/{id:guid}", async (
            Guid id,
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

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
            {
                return Results.NotFound();
            }

            var workspace = await db.ClaimWorkspaces
                .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

            if (workspace is null)
            {
                return Results.NotFound();
            }

            workspace.Status = "ARCHIVED";
            workspace.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspace.Id,
                "CLAIM_WORKSPACE_ARCHIVED",
                $"Claim workspace archived. WorkspaceId={workspace.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = workspace.Id,
                status = workspace.Status,
                archived = true
            });
        });

        return app;
    }

    internal static object ToClaimWorkspaceResponse(ClaimWorkspace workspace)
    {
        return new
        {
            id = workspace.Id,
            userId = workspace.UserId,
            claimFramework = workspace.ClaimFramework,
            claimScenario = workspace.ClaimScenario,
            workspaceTitle = workspace.WorkspaceTitle,
            status = workspace.Status,
            generatedPackStatus = workspace.GeneratedPackStatus,
            createdAt = workspace.CreatedAt,
            updatedAt = workspace.UpdatedAt,
            lastOpenedAt = workspace.LastOpenedAt
        };
    }

    private static string NormaliseClaimScenario(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidClaimScenario(string value) => GetAllowedClaimScenarios().Contains(value);

    private static bool IsValidWorkspaceStatus(string value) => GetAllowedWorkspaceStatuses().Contains(value);

    private static string[] GetAllowedClaimScenarios()
    {
        return new[]
        {
            "NEW_CONDITION",
            "WORSENING_EXISTING_CONDITION",
            "NEW_PLUS_EXISTING",
            "EVIDENCE_PREP_ONLY",
            "UNSURE"
        };
    }

    private static string[] GetAllowedWorkspaceStatuses()
    {
        return new[]
        {
            "NOT_STARTED",
            "IN_PROGRESS",
            "EVIDENCE_GAPS_FOUND",
            "DRAFTS_READY_FOR_REVIEW",
            "READY_TO_GENERATE",
            "PACK_GENERATED",
            "ARCHIVED"
        };
    }
}

public sealed record CreateClaimWorkspaceRequest(string? ClaimScenario, string? WorkspaceTitle);

public sealed record UpdateClaimWorkspaceRequest(string? ClaimScenario, string? WorkspaceTitle, string? Status);
