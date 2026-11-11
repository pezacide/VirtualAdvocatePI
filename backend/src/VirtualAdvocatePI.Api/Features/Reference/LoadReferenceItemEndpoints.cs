using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.Reference;

public static class LoadReferenceItemEndpoints
{
    public static IEndpointRouteBuilder MapLoadReferenceItemEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reference/load-reference-items", async (
            string? category,
            HttpRequest request,
            CurrentUserService currentUserService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var query = db.LoadReferenceItems
                .Where(x => x.Status == "ACTIVE");

            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalisedCategory = category.Trim().ToUpperInvariant();
                query = query.Where(x => x.Category == normalisedCategory);
            }

            var rows = await query
                .OrderBy(x => x.Category)
                .ThenBy(x => x.ItemName)
                .ToListAsync();

            return Results.Ok(new
            {
                referenceGuidanceOnly = true,
                note = "Approximate typical weights for preparation only. Confirm against records or the relevant reference where accuracy matters.",
                items = rows.Select(ToResponse).ToList()
            });
        });

        return app;
    }

    internal static object ToResponse(LoadReferenceItem item)
    {
        return new
        {
            id = item.Id,
            itemName = item.ItemName,
            category = item.Category,
            typicalWeightKg = item.TypicalWeightKg,
            weightRangeKg = item.WeightRangeKg,
            serviceContext = item.ServiceContext,
            notes = item.Notes,
            sourceLabel = item.SourceLabel,
            status = item.Status,
            createdAt = item.CreatedAt,
            updatedAt = item.UpdatedAt
        };
    }
}
