using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminAccessEndpoints
{
    public static IEndpointRouteBuilder MapAdminAccessEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/me", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var access = adminAccessService.GetAccess(user);

            return Results.Ok(new
            {
                userId = user.Id,
                firebaseUid = user.FirebaseUid,
                email = user.Email,
                displayName = user.DisplayName,
                role = user.Role,
                accountStatus = user.AccountStatus,
                isAdmin = access.IsAdmin,
                reason = access.Reason
            });
        });

        app.MapGet("/api/v1/admin/ping", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService) =>
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

            return Results.Ok(new
            {
                ok = true,
                message = "Admin endpoint access granted.",
                userId = user.Id,
                email = user.Email,
                role = user.Role
            });
        });

        return app;
    }
}