using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Admin;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminPromptDisclaimerVersionEndpoints
{
    public static IEndpointRouteBuilder MapAdminPromptDisclaimerVersionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/prompt-disclaimer-versions", async (
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
            var versionType = request.Query["versionType"].ToString();
            var category = request.Query["category"].ToString();
            var appliesTo = request.Query["appliesTo"].ToString();
            var approvalStatus = request.Query["approvalStatus"].ToString();
            var status = request.Query["status"].ToString();

            var query = db.AdminPromptDisclaimerVersionEntries
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.Trim().ToLower();

                query = query.Where(x =>
                    x.VersionKey.ToLower().Contains(lowered) ||
                    x.Title.ToLower().Contains(lowered) ||
                    x.Description.ToLower().Contains(lowered) ||
                    x.Content.ToLower().Contains(lowered));
            }

            if (!string.IsNullOrWhiteSpace(versionType))
            {
                query = query.Where(x => x.VersionType == NormaliseVersionType(versionType));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(appliesTo))
            {
                query = query.Where(x => x.AppliesTo == appliesTo.Trim());
            }

            if (!string.IsNullOrWhiteSpace(approvalStatus))
            {
                query = query.Where(x => x.ApprovalStatus == NormaliseApprovalStatus(approvalStatus));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == NormaliseStatus(status));
            }

            var rows = await query
                .OrderBy(x => x.VersionType)
                .ThenBy(x => x.Category)
                .ThenBy(x => x.VersionKey)
                .Take(250)
                .ToListAsync(cancellationToken);

            return Results.Ok(rows.Select(ToResponse));
        });

        app.MapPost("/api/v1/admin/prompt-disclaimer-versions", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            CreateAdminPromptDisclaimerVersionRequest input,
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

            var versionKey = Required(input.VersionKey, "VersionKey").Trim();

            var exists = await db.AdminPromptDisclaimerVersionEntries
                .AnyAsync(x => x.VersionKey == versionKey, cancellationToken);

            if (exists)
            {
                return Results.BadRequest(new
                {
                    error = "A prompt/disclaimer version with this VersionKey already exists.",
                    versionKey
                });
            }

            var now = DateTimeOffset.UtcNow;

            var entry = new AdminPromptDisclaimerVersionEntry
            {
                Id = Guid.NewGuid(),
                VersionKey = versionKey,
                VersionType = NormaliseVersionType(input.VersionType ?? "PROMPT"),
                Title = Required(input.Title, "Title"),
                Description = input.Description?.Trim() ?? string.Empty,
                Category = RequiredOrFallback(input.Category, "GENERAL"),
                VersionLabel = RequiredOrFallback(input.VersionLabel, "v1"),
                AppliesTo = RequiredOrFallback(input.AppliesTo, "GENERAL"),
                Content = input.Content?.Trim() ?? string.Empty,
                ApprovalStatus = NormaliseApprovalStatus(input.ApprovalStatus ?? "DRAFT"),
                ApprovedBy = NullIfWhiteSpace(input.ApprovedBy),
                ReviewNotes = NullIfWhiteSpace(input.ReviewNotes),
                EffectiveFrom = input.EffectiveFrom,
                RetiredAt = input.RetiredAt,
                IsActive = input.IsActive ?? true,
                Status = NormaliseStatus(input.Status ?? "ACTIVE"),
                CreatedAt = now,
                UpdatedAt = now
            };

            if (entry.ApprovalStatus == "APPROVED" && string.IsNullOrWhiteSpace(entry.ApprovedBy))
            {
                entry.ApprovedBy = user.Email;
            }

            db.AdminPromptDisclaimerVersionEntries.Add(entry);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(entry));
        });

        app.MapPatch("/api/v1/admin/prompt-disclaimer-versions/{id:guid}", async (
            Guid id,
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            UpdateAdminPromptDisclaimerVersionRequest input,
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

            var entry = await db.AdminPromptDisclaimerVersionEntries
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entry is null)
            {
                return Results.NotFound();
            }

            if (input.Title is not null)
            {
                entry.Title = input.Title.Trim();
            }

            if (input.Description is not null)
            {
                entry.Description = input.Description.Trim();
            }

            if (input.Category is not null)
            {
                entry.Category = input.Category.Trim();
            }

            if (input.VersionLabel is not null)
            {
                entry.VersionLabel = input.VersionLabel.Trim();
            }

            if (input.AppliesTo is not null)
            {
                entry.AppliesTo = input.AppliesTo.Trim();
            }

            if (input.Content is not null)
            {
                entry.Content = input.Content;
            }

            if (input.ApprovalStatus is not null)
            {
                entry.ApprovalStatus = NormaliseApprovalStatus(input.ApprovalStatus);

                if (entry.ApprovalStatus == "APPROVED" && string.IsNullOrWhiteSpace(entry.ApprovedBy))
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

            if (input.EffectiveFromSet.HasValue && input.EffectiveFromSet.Value)
            {
                entry.EffectiveFrom = input.EffectiveFrom;
            }

            if (input.RetiredAtSet.HasValue && input.RetiredAtSet.Value)
            {
                entry.RetiredAt = input.RetiredAt;
            }

            if (input.IsActive.HasValue)
            {
                entry.IsActive = input.IsActive.Value;
            }

            if (input.Status is not null)
            {
                entry.Status = NormaliseStatus(input.Status);
            }

            entry.UpdatedAt = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(entry));
        });

        return app;
    }

    private static object ToResponse(AdminPromptDisclaimerVersionEntry entry)
    {
        return new
        {
            id = entry.Id,
            versionKey = entry.VersionKey,
            versionType = entry.VersionType,
            title = entry.Title,
            description = entry.Description,
            category = entry.Category,
            versionLabel = entry.VersionLabel,
            appliesTo = entry.AppliesTo,
            content = entry.Content,
            approvalStatus = entry.ApprovalStatus,
            approvedBy = entry.ApprovedBy,
            reviewNotes = entry.ReviewNotes,
            effectiveFrom = entry.EffectiveFrom,
            retiredAt = entry.RetiredAt,
            isActive = entry.IsActive,
            status = entry.Status,
            createdAt = entry.CreatedAt,
            updatedAt = entry.UpdatedAt
        };
    }

    private static string Required(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{fieldName} is required.");
        }

        return value.Trim();
    }

    private static string RequiredOrFallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim();
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string NormaliseVersionType(string value)
    {
        var normalised = value.Trim().ToUpperInvariant();

        return normalised is "PROMPT" or "DISCLAIMER"
            ? normalised
            : throw new InvalidOperationException("VersionType must be PROMPT or DISCLAIMER.");
    }

    private static string NormaliseApprovalStatus(string value)
    {
        var normalised = value.Trim().ToUpperInvariant();

        return normalised is "DRAFT" or "PENDING_REVIEW" or "APPROVED" or "REJECTED"
            ? normalised
            : throw new InvalidOperationException("ApprovalStatus must be DRAFT, PENDING_REVIEW, APPROVED or REJECTED.");
    }

    private static string NormaliseStatus(string value)
    {
        var normalised = value.Trim().ToUpperInvariant();

        return normalised is "ACTIVE" or "ARCHIVED"
            ? normalised
            : throw new InvalidOperationException("Status must be ACTIVE or ARCHIVED.");
    }
}

public sealed record CreateAdminPromptDisclaimerVersionRequest(
    string? VersionKey,
    string? VersionType,
    string? Title,
    string? Description,
    string? Category,
    string? VersionLabel,
    string? AppliesTo,
    string? Content,
    string? ApprovalStatus,
    string? ApprovedBy,
    string? ReviewNotes,
    DateTimeOffset? EffectiveFrom,
    DateTimeOffset? RetiredAt,
    bool? IsActive,
    string? Status
);

public sealed record UpdateAdminPromptDisclaimerVersionRequest(
    string? Title,
    string? Description,
    string? Category,
    string? VersionLabel,
    string? AppliesTo,
    string? Content,
    string? ApprovalStatus,
    string? ApprovedBy,
    string? ReviewNotes,
    DateTimeOffset? EffectiveFrom,
    bool? EffectiveFromSet,
    DateTimeOffset? RetiredAt,
    bool? RetiredAtSet,
    bool? IsActive,
    string? Status
);