using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminDatabaseMaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapAdminDatabaseMaintenanceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/admin/database/apply-migrations", async (
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

            var pendingBefore = await db.Database.GetPendingMigrationsAsync(cancellationToken);

            await db.Database.MigrateAsync(cancellationToken);

            var pendingAfter = await db.Database.GetPendingMigrationsAsync(cancellationToken);

            return Results.Ok(new
            {
                applied = true,
                requestedBy = user.Email,
                pendingBefore = pendingBefore.ToArray(),
                pendingAfter = pendingAfter.ToArray(),
                message = "Database migrations applied."
            });
        });

        return app;
    }
}