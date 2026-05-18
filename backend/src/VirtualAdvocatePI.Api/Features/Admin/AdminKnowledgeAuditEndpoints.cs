using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminKnowledgeAuditEndpoints
{
    public static IEndpointRouteBuilder MapAdminKnowledgeAuditEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/knowledge-audit", async (
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

            if (!adminAccessService.IsAdmin(user))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var search = request.Query["search"].ToString();
            var eventType = request.Query["eventType"].ToString();
            var workspaceIdRaw = request.Query["workspaceId"].ToString();
            var userIdRaw = request.Query["userId"].ToString();
            var fromRaw = request.Query["from"].ToString();
            var toRaw = request.Query["to"].ToString();
            var knowledgeOnlyRaw = request.Query["knowledgeOnly"].ToString();

            var query = db.AuditEvents
                .AsNoTracking()
                .AsQueryable();

            if (bool.TryParse(knowledgeOnlyRaw, out var knowledgeOnly) && knowledgeOnly)
            {
                query = query.Where(x =>
                    x.EventType.StartsWith("AI_") ||
                    x.EventType.StartsWith("RAG_") ||
                    x.EventType.Contains("SOURCE") ||
                    x.EventType.Contains("TEMPLATE") ||
                    x.EventType.Contains("PROMPT") ||
                    x.EventType.Contains("DISCLAIMER") ||
                    x.EventType.Contains("KNOWLEDGE") ||
                    x.EventType.Contains("GENERATED_DOCUMENT") ||
                    x.EventType.Contains("CLAIM_STARTER_PACK") ||
                    x.EventType.Contains("DOCTOR_GUIDANCE_PACK") ||
                    x.EventType.Contains("DATABASE") ||
                    x.EventType.Contains("SEED"));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.Trim().ToLower();

                query = query.Where(x =>
                    x.EventType.ToLower().Contains(lowered) ||
                    (x.EventDetail != null && x.EventDetail.ToLower().Contains(lowered)) ||
                    (x.ClientType != null && x.ClientType.ToLower().Contains(lowered)) ||
                    (x.IpAddress != null && x.IpAddress.ToLower().Contains(lowered)));
            }

            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(x => x.EventType == eventType.Trim());
            }

            if (Guid.TryParse(workspaceIdRaw, out var workspaceId))
            {
                query = query.Where(x => x.ClaimWorkspaceId == workspaceId);
            }

            if (Guid.TryParse(userIdRaw, out var userId))
            {
                query = query.Where(x => x.UserId == userId);
            }

            if (DateTimeOffset.TryParse(fromRaw, out var from))
            {
                query = query.Where(x => x.CreatedAt >= from);
            }

            if (DateTimeOffset.TryParse(toRaw, out var to))
            {
                query = query.Where(x => x.CreatedAt <= to);
            }

            var rows = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(500)
                .ToListAsync(cancellationToken);

            var eventTypeSummary = rows
                .GroupBy(x => x.EventType)
                .Select(x => new
                {
                    eventType = x.Key,
                    count = x.Count()
                })
                .OrderByDescending(x => x.count)
                .ThenBy(x => x.eventType)
                .ToList();

            return Results.Ok(new
            {
                totalReturned = rows.Count,
                eventTypeSummary,
                rows = rows.Select(ToResponse)
            });
        });

        app.MapGet("/api/v1/admin/knowledge-audit/{auditEventId:guid}", async (
            Guid auditEventId,
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

            if (!adminAccessService.IsAdmin(user))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var auditEvent = await db.AuditEvents
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == auditEventId, cancellationToken);

            if (auditEvent is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToResponse(auditEvent));
        });

        return app;
    }

    private static object ToResponse(AuditEvent auditEvent)
    {
        return new
        {
            id = auditEvent.Id,
            userId = auditEvent.UserId,
            claimWorkspaceId = auditEvent.ClaimWorkspaceId,
            eventType = auditEvent.EventType,
            eventDetail = auditEvent.EventDetail,
            ipAddress = auditEvent.IpAddress,
            clientType = auditEvent.ClientType,
            createdAt = auditEvent.CreatedAt
        };
    }
}