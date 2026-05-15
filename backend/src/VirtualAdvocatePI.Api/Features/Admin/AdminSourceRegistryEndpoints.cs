using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminSourceRegistryEndpoints
{
    public static IEndpointRouteBuilder MapAdminSourceRegistryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/source-registry", async (
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

            var search = request.Query["search"].ToString();
            var category = request.Query["category"].ToString();
            var sourceType = request.Query["sourceType"].ToString();
            var approvalStatus = request.Query["approvalStatus"].ToString();
            var status = request.Query["status"].ToString();
            var isActiveRaw = request.Query["isActive"].ToString();

            var query = db.AiSourceRegistryEntries
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var loweredSearch = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SourceKey.ToLower().Contains(loweredSearch) ||
                    x.Title.ToLower().Contains(loweredSearch) ||
                    x.CitationLabel.ToLower().Contains(loweredSearch));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(sourceType))
            {
                query = query.Where(x => x.SourceType == sourceType.Trim());
            }

            if (!string.IsNullOrWhiteSpace(approvalStatus))
            {
                query = query.Where(x => x.ApprovalStatus == NormaliseUpper(approvalStatus));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == NormaliseUpper(status));
            }

            if (bool.TryParse(isActiveRaw, out var isActive))
            {
                query = query.Where(x => x.IsActive == isActive);
            }

            var rows = await query
                .OrderBy(x => x.SourceKey)
                .Take(250)
                .ToListAsync(cancellationToken);

            return Results.Ok(rows.Select(ToResponse));
        });

        app.MapGet("/api/v1/admin/source-registry/{id:guid}", async (
            Guid id,
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

            var entry = await db.AiSourceRegistryEntries
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entry is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToResponse(entry));
        });

        app.MapPatch("/api/v1/admin/source-registry/{id:guid}", async (
            Guid id,
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            UpdateAdminSourceRegistryEntryRequest input,
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

            var entry = await db.AiSourceRegistryEntries
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entry is null)
            {
                return Results.NotFound();
            }

            if (input.Title is not null)
            {
                entry.Title = input.Title.Trim();
            }

            if (input.Category is not null)
            {
                entry.Category = input.Category.Trim();
            }

            if (input.SourceType is not null)
            {
                entry.SourceType = input.SourceType.Trim();
            }

            if (input.Jurisdiction is not null)
            {
                entry.Jurisdiction = input.Jurisdiction.Trim();
            }

            if (input.SourceVersion is not null)
            {
                entry.SourceVersion = NullIfWhiteSpace(input.SourceVersion);
            }

            if (input.CitationLabel is not null)
            {
                entry.CitationLabel = input.CitationLabel.Trim();
            }

            if (input.SourceUrl is not null)
            {
                entry.SourceUrl = NullIfWhiteSpace(input.SourceUrl);
            }

            if (input.StoragePath is not null)
            {
                entry.StoragePath = NullIfWhiteSpace(input.StoragePath);
            }

            if (input.ContentHash is not null)
            {
                entry.ContentHash = NullIfWhiteSpace(input.ContentHash);
            }

            if (input.ApprovalStatus is not null)
            {
                var approvalStatus = NormaliseUpper(input.ApprovalStatus);

                if (!IsValidApprovalStatus(approvalStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid approval status.",
                        allowedValues = new[] { "DRAFT", "PENDING_REVIEW", "APPROVED", "REJECTED" }
                    });
                }

                entry.ApprovalStatus = approvalStatus;

                if (approvalStatus == "APPROVED" && string.IsNullOrWhiteSpace(entry.ApprovedBy))
                {
                    entry.ApprovedBy = user.Email;
                }
            }

            if (input.ApprovedBy is not null)
            {
                entry.ApprovedBy = NullIfWhiteSpace(input.ApprovedBy);
            }

            if (input.ReviewNotes is not null)
            {
                entry.ReviewNotes = NullIfWhiteSpace(input.ReviewNotes);
            }

            if (input.Status is not null)
            {
                var status = NormaliseUpper(input.Status);

                if (!IsValidStatus(status))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid source registry status.",
                        allowedValues = new[] { "ACTIVE", "ARCHIVED" }
                    });
                }

                entry.Status = status;
            }

            if (input.IsActive.HasValue)
            {
                entry.IsActive = input.IsActive.Value;
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(entry));
        });

        return app;
    }

    private static object ToResponse(AiSourceRegistryEntry entry)
    {
        return new
        {
            id = entry.Id,
            sourceKey = entry.SourceKey,
            title = entry.Title,
            category = entry.Category,
            sourceType = entry.SourceType,
            jurisdiction = entry.Jurisdiction,
            sourceVersion = entry.SourceVersion,
            citationLabel = entry.CitationLabel,
            sourceUrl = entry.SourceUrl,
            storagePath = entry.StoragePath,
            contentHash = entry.ContentHash,
            approvalStatus = entry.ApprovalStatus,
            approvedBy = entry.ApprovedBy,
            reviewNotes = entry.ReviewNotes,
            isActive = entry.IsActive,
            status = entry.Status,
            createdAt = entry.CreatedAt,
            updatedAt = entry.UpdatedAt
        };
    }

    private static string NormaliseUpper(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NullIfWhiteSpace(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool IsValidApprovalStatus(string value)
    {
        return value is "DRAFT" or "PENDING_REVIEW" or "APPROVED" or "REJECTED";
    }

    private static bool IsValidStatus(string value)
    {
        return value is "ACTIVE" or "ARCHIVED";
    }
}

public sealed record UpdateAdminSourceRegistryEntryRequest(
    string? Title,
    string? Category,
    string? SourceType,
    string? Jurisdiction,
    string? SourceVersion,
    string? CitationLabel,
    string? SourceUrl,
    string? StoragePath,
    string? ContentHash,
    string? ApprovalStatus,
    string? ApprovedBy,
    string? ReviewNotes,
    bool? IsActive,
    string? Status
);