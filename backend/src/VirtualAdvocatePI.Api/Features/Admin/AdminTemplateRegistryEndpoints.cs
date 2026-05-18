using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Admin;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Admin;

public static class AdminTemplateRegistryEndpoints
{
    public static IEndpointRouteBuilder MapAdminTemplateRegistryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/templates", async (
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
            var templateType = request.Query["templateType"].ToString();
            var category = request.Query["category"].ToString();
            var approvalStatus = request.Query["approvalStatus"].ToString();
            var status = request.Query["status"].ToString();

            var query = db.AdminTemplateRegistryEntries
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowered = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TemplateKey.ToLower().Contains(lowered) ||
                    x.Title.ToLower().Contains(lowered) ||
                    x.Description.ToLower().Contains(lowered));
            }

            if (!string.IsNullOrWhiteSpace(templateType))
            {
                query = query.Where(x => x.TemplateType == NormaliseUpper(templateType));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category == category.Trim());
            }

            if (!string.IsNullOrWhiteSpace(approvalStatus))
            {
                query = query.Where(x => x.ApprovalStatus == NormaliseUpper(approvalStatus));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == NormaliseUpper(status));
            }

            var rows = await query
                .OrderBy(x => x.TemplateType)
                .ThenBy(x => x.TemplateKey)
                .Take(250)
                .ToListAsync(cancellationToken);

            return Results.Ok(rows.Select(ToResponse));
        });

        app.MapPost("/api/v1/admin/templates", async (
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            CreateAdminTemplateRegistryEntryRequest input,
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

            var templateKey = Required(input.TemplateKey, "TemplateKey").Trim();

            var exists = await db.AdminTemplateRegistryEntries
                .AnyAsync(x => x.TemplateKey == templateKey, cancellationToken);

            if (exists)
            {
                return Results.BadRequest(new
                {
                    error = "A template with this TemplateKey already exists.",
                    templateKey
                });
            }

            var now = DateTimeOffset.UtcNow;

            var entry = new AdminTemplateRegistryEntry
            {
                Id = Guid.NewGuid(),
                TemplateKey = templateKey,
                TemplateType = NormaliseTemplateType(input.TemplateType),
                Title = Required(input.Title, "Title"),
                Description = input.Description?.Trim() ?? string.Empty,
                Category = RequiredOrFallback(input.Category, "GENERAL"),
                TemplateVersion = RequiredOrFallback(input.TemplateVersion, "v1"),
                TemplateBody = input.TemplateBody?.Trim() ?? string.Empty,
                OutputFormat = RequiredOrFallback(input.OutputFormat, "TEXT").ToUpperInvariant(),
                ApprovalStatus = NormaliseApprovalStatus(input.ApprovalStatus ?? "DRAFT"),
                ApprovedBy = NullIfWhiteSpace(input.ApprovedBy),
                ReviewNotes = NullIfWhiteSpace(input.ReviewNotes),
                IsActive = input.IsActive ?? true,
                Status = NormaliseStatus(input.Status ?? "ACTIVE"),
                CreatedAt = now,
                UpdatedAt = now
            };

            if (entry.ApprovalStatus == "APPROVED" && string.IsNullOrWhiteSpace(entry.ApprovedBy))
            {
                entry.ApprovedBy = user.Email;
            }

            db.AdminTemplateRegistryEntries.Add(entry);
            await db.SaveChangesAsync(cancellationToken);

            return Results.Ok(ToResponse(entry));
        });

        app.MapPatch("/api/v1/admin/templates/{id:guid}", async (
            Guid id,
            HttpRequest request,
            CurrentUserService currentUserService,
            AdminAccessService adminAccessService,
            VirtualAdvocateDbContext db,
            UpdateAdminTemplateRegistryEntryRequest input,
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

            var entry = await db.AdminTemplateRegistryEntries
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

            if (input.TemplateVersion is not null)
            {
                entry.TemplateVersion = input.TemplateVersion.Trim();
            }

            if (input.TemplateBody is not null)
            {
                entry.TemplateBody = input.TemplateBody;
            }

            if (input.OutputFormat is not null)
            {
                entry.OutputFormat = input.OutputFormat.Trim().ToUpperInvariant();
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

    private static object ToResponse(AdminTemplateRegistryEntry entry)
    {
        return new
        {
            id = entry.Id,
            templateKey = entry.TemplateKey,
            templateType = entry.TemplateType,
            title = entry.Title,
            description = entry.Description,
            category = entry.Category,
            templateVersion = entry.TemplateVersion,
            templateBody = entry.TemplateBody,
            outputFormat = entry.OutputFormat,
            approvalStatus = entry.ApprovalStatus,
            approvedBy = entry.ApprovedBy,
            reviewNotes = entry.ReviewNotes,
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

    private static string NormaliseUpper(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string NormaliseTemplateType(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value)
            ? "QUESTION"
            : value.Trim().ToUpperInvariant();

        return normalised is "QUESTION" or "DOCUMENT"
            ? normalised
            : throw new InvalidOperationException("TemplateType must be QUESTION or DOCUMENT.");
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

public sealed record CreateAdminTemplateRegistryEntryRequest(
    string? TemplateKey,
    string? TemplateType,
    string? Title,
    string? Description,
    string? Category,
    string? TemplateVersion,
    string? TemplateBody,
    string? OutputFormat,
    string? ApprovalStatus,
    string? ApprovedBy,
    string? ReviewNotes,
    bool? IsActive,
    string? Status
);

public sealed record UpdateAdminTemplateRegistryEntryRequest(
    string? Title,
    string? Description,
    string? Category,
    string? TemplateVersion,
    string? TemplateBody,
    string? OutputFormat,
    string? ApprovalStatus,
    string? ApprovedBy,
    string? ReviewNotes,
    bool? IsActive,
    string? Status
);