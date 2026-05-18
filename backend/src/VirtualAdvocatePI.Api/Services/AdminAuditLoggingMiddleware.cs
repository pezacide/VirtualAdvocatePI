using VirtualAdvocatePI.Api.Data;

namespace VirtualAdvocatePI.Api.Services;

public sealed class AdminAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public AdminAuditLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        CurrentUserService currentUserService,
        AdminAccessService adminAccessService,
        AuditService auditService,
        VirtualAdvocateDbContext db)
    {
        await _next(context);

        if (!ShouldLog(context))
        {
            return;
        }

        var user = await currentUserService.GetOrCreateCurrentUserAsync(context.Request);

        if (user is null)
        {
            return;
        }

        if (!adminAccessService.IsAdmin(user))
        {
            return;
        }

        var eventType = BuildEventType(context.Request);
        var detail = BuildEventDetail(context, user.Email);

        auditService.AddAdminAuditEvent(
            context.Request,
            user.Id,
            eventType,
            detail);

        await db.SaveChangesAsync(context.RequestAborted);
    }

    private static bool ShouldLog(HttpContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (!request.Path.StartsWithSegments("/api/v1/admin"))
        {
            return false;
        }

        if (response.StatusCode < 200 || response.StatusCode >= 300)
        {
            return false;
        }

        return HttpMethods.IsPost(request.Method) ||
               HttpMethods.IsPatch(request.Method) ||
               HttpMethods.IsDelete(request.Method);
    }

    private static string BuildEventType(HttpRequest request)
    {
        var path = request.Path.Value ?? string.Empty;
        var method = request.Method.ToUpperInvariant();

        if (path.Equals("/api/v1/admin/database/apply-migrations", StringComparison.OrdinalIgnoreCase))
        {
            return "ADMIN_DATABASE_MIGRATIONS_APPLIED";
        }

        if (path.Equals("/api/v1/admin/source-registry/seed-approved", StringComparison.OrdinalIgnoreCase))
        {
            return "ADMIN_SOURCE_REGISTRY_SEEDED";
        }

        if (path.StartsWith("/api/v1/admin/source-registry/", StringComparison.OrdinalIgnoreCase) &&
            method == "PATCH")
        {
            return "ADMIN_SOURCE_REGISTRY_UPDATED";
        }

        if (path.Equals("/api/v1/admin/templates", StringComparison.OrdinalIgnoreCase) &&
            method == "POST")
        {
            return "ADMIN_TEMPLATE_CREATED";
        }

        if (path.StartsWith("/api/v1/admin/templates/", StringComparison.OrdinalIgnoreCase) &&
            method == "PATCH")
        {
            return "ADMIN_TEMPLATE_UPDATED";
        }

        if (path.Equals("/api/v1/admin/prompt-disclaimer-versions", StringComparison.OrdinalIgnoreCase) &&
            method == "POST")
        {
            return "ADMIN_PROMPT_DISCLAIMER_VERSION_CREATED";
        }

        if (path.StartsWith("/api/v1/admin/prompt-disclaimer-versions/", StringComparison.OrdinalIgnoreCase) &&
            method == "PATCH")
        {
            return "ADMIN_PROMPT_DISCLAIMER_VERSION_UPDATED";
        }

        return "ADMIN_WRITE_ACTION";
    }

    private static string BuildEventDetail(HttpContext context, string? email)
    {
        var request = context.Request;
        var response = context.Response;

        var query = request.QueryString.HasValue
            ? request.QueryString.Value
            : string.Empty;

        return string.Join("; ", new[]
        {
            $"AdminEmail={email ?? "unknown"}",
            $"Method={request.Method}",
            $"Path={request.Path}",
            $"Query={query}",
            $"StatusCode={response.StatusCode}",
            "RequestBodyLogged=false"
        });
    }
}