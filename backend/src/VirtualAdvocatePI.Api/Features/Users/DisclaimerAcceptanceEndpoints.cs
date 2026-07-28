using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Users;

public static class DisclaimerAcceptanceEndpoints
{
    public const string DisclaimerAcceptedEventType = "DISCLAIMER_ACCEPTED";

    public static IEndpointRouteBuilder MapDisclaimerAcceptanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/me/disclaimer-acceptance", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var accepted = await db.AuditEvents
                .AnyAsync(x => x.UserId == user.Id && x.EventType == DisclaimerAcceptedEventType);

            return Results.Ok(new { accepted });
        });

        app.MapPost("/api/v1/me/disclaimer-acceptance", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AuditService auditService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            auditService.AddAuditEvent(
                request,
                user.Id,
                Guid.Empty,
                DisclaimerAcceptedEventType,
                "User acknowledged the preparation-support-only disclaimer.");

            await db.SaveChangesAsync();

            return Results.Ok(new { accepted = true });
        });

        return app;
    }
}
