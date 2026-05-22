using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Mobile;

public static class MobileSessionEndpoints
{
    public static IEndpointRouteBuilder MapMobileSessionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/mobile/me", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var adminAccess = adminAccessService.GetAccess(user);

            return Results.Ok(new
            {
                userId = user.Id,
                firebaseUid = user.FirebaseUid,
                email = user.Email,
                displayName = user.DisplayName,
                role = user.Role,
                accountStatus = user.AccountStatus,
                isAdmin = adminAccess.IsAdmin,
                authenticated = true
            });
        });

        return app;
    }
}