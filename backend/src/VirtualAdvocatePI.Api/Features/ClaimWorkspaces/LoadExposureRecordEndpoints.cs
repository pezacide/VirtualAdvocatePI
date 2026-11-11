using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class LoadExposureRecordEndpoints
{
    public static IEndpointRouteBuilder MapLoadExposureRecordEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/load-exposure-records", async (
            Guid workspaceId,
            Guid? conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var query = db.LoadExposureRecords
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED");

            if (conditionId is { } filterConditionId)
            {
                query = query.Where(x => x.ConditionId == filterConditionId);
            }

            var rows = await query
                .OrderBy(x => x.RecordType)
                .ThenByDescending(x => x.UpdatedAt)
                .ToListAsync();

            return Results.Ok(rows.Select(ToResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/load-exposure-records", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateLoadExposureRecordRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var recordType = NormaliseRecordType(input.RecordType);

            if (!IsValidRecordType(recordType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid load exposure record type.",
                    allowedValues = GetAllowedRecordTypes()
                });
            }

            if (string.IsNullOrWhiteSpace(input.ActivityDescription))
            {
                return Results.BadRequest(new { error = "An activity description is required." });
            }

            if (input.ConditionId is { } conditionId &&
                !await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.BadRequest(new { error = "The linked condition was not found in this workspace." });
            }

            var record = new LoadExposureRecord
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = input.ConditionId,
                RecordType = recordType,
                ActivityDescription = input.ActivityDescription.Trim(),
                BodyAreaAffected = NormaliseBodyArea(input.BodyAreaAffected),
                TypicalWeightKg = SanitiseWeight(input.TypicalWeightKg),
                MaxWeightKg = SanitiseWeight(input.MaxWeightKg),
                Frequency = NormaliseFrequency(input.Frequency),
                DurationPerOccasion = Trimmed(input.DurationPerOccasion),
                RepetitionsDescription = Trimmed(input.RepetitionsDescription),
                ServicePeriodFrom = input.ServicePeriodFrom,
                ServicePeriodTo = input.ServicePeriodTo,
                YearsExposed = SanitiseYears(input.YearsExposed),
                EquipmentOrContext = Trimmed(input.EquipmentOrContext),
                HazardType = recordType == "HAZARD_EXPOSURE" ? NormaliseHazardType(input.HazardType) : null,
                Notes = Trimmed(input.Notes),
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.LoadExposureRecords.Add(record);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "LOAD_EXPOSURE_RECORD_CREATED",
                $"Load exposure record created. RecordId={record.Id}; RecordType={recordType}; ConditionId={input.ConditionId}");

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/claim-workspaces/{workspaceId}/load-exposure-records/{record.Id}",
                ToResponse(record));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/load-exposure-records/{recordId:guid}", async (
            Guid workspaceId,
            Guid recordId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateLoadExposureRecordRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var record = await db.LoadExposureRecords
                .FirstOrDefaultAsync(x =>
                    x.Id == recordId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (record is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.RecordType))
            {
                var recordType = NormaliseRecordType(input.RecordType);

                if (!IsValidRecordType(recordType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid load exposure record type.",
                        allowedValues = GetAllowedRecordTypes()
                    });
                }

                record.RecordType = recordType;

                if (recordType != "HAZARD_EXPOSURE")
                {
                    record.HazardType = null;
                }
            }

            if (!string.IsNullOrWhiteSpace(input.ActivityDescription))
            {
                record.ActivityDescription = input.ActivityDescription.Trim();
            }

            if (!string.IsNullOrWhiteSpace(input.BodyAreaAffected))
            {
                record.BodyAreaAffected = NormaliseBodyArea(input.BodyAreaAffected);
            }

            if (!string.IsNullOrWhiteSpace(input.Frequency))
            {
                record.Frequency = NormaliseFrequency(input.Frequency);
            }

            if (input.HazardType is not null && record.RecordType == "HAZARD_EXPOSURE")
            {
                record.HazardType = NormaliseHazardType(input.HazardType);
            }

            record.TypicalWeightKg = input.TypicalWeightKg.HasValue ? SanitiseWeight(input.TypicalWeightKg) : record.TypicalWeightKg;
            record.MaxWeightKg = input.MaxWeightKg.HasValue ? SanitiseWeight(input.MaxWeightKg) : record.MaxWeightKg;
            record.YearsExposed = input.YearsExposed.HasValue ? SanitiseYears(input.YearsExposed) : record.YearsExposed;
            record.DurationPerOccasion = input.DurationPerOccasion is null ? record.DurationPerOccasion : Trimmed(input.DurationPerOccasion);
            record.RepetitionsDescription = input.RepetitionsDescription is null ? record.RepetitionsDescription : Trimmed(input.RepetitionsDescription);
            record.ServicePeriodFrom = input.ServicePeriodFrom ?? record.ServicePeriodFrom;
            record.ServicePeriodTo = input.ServicePeriodTo ?? record.ServicePeriodTo;
            record.EquipmentOrContext = input.EquipmentOrContext is null ? record.EquipmentOrContext : Trimmed(input.EquipmentOrContext);
            record.Notes = input.Notes is null ? record.Notes : Trimmed(input.Notes);
            record.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "LOAD_EXPOSURE_RECORD_UPDATED",
                $"Load exposure record updated. RecordId={record.Id}; RecordType={record.RecordType}");

            await db.SaveChangesAsync();

            return Results.Ok(ToResponse(record));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/load-exposure-records/{recordId:guid}", async (
            Guid workspaceId,
            Guid recordId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, workspaceId))
            {
                return Results.NotFound();
            }

            var record = await db.LoadExposureRecords
                .FirstOrDefaultAsync(x =>
                    x.Id == recordId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.Status != "ARCHIVED");

            if (record is null)
            {
                return Results.NotFound();
            }

            record.Status = "ARCHIVED";
            record.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "LOAD_EXPOSURE_RECORD_ARCHIVED",
                $"Load exposure record archived. RecordId={record.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new { id = record.Id, status = record.Status, archived = true });
        });

        return app;
    }

    internal static object ToResponse(LoadExposureRecord record)
    {
        return new
        {
            id = record.Id,
            claimWorkspaceId = record.ClaimWorkspaceId,
            conditionId = record.ConditionId,
            recordType = record.RecordType,
            activityDescription = record.ActivityDescription,
            bodyAreaAffected = record.BodyAreaAffected,
            typicalWeightKg = record.TypicalWeightKg,
            maxWeightKg = record.MaxWeightKg,
            frequency = record.Frequency,
            durationPerOccasion = record.DurationPerOccasion,
            repetitionsDescription = record.RepetitionsDescription,
            servicePeriodFrom = record.ServicePeriodFrom,
            servicePeriodTo = record.ServicePeriodTo,
            yearsExposed = record.YearsExposed,
            equipmentOrContext = record.EquipmentOrContext,
            hazardType = record.HazardType,
            notes = record.Notes,
            status = record.Status,
            createdAt = record.CreatedAt,
            updatedAt = record.UpdatedAt
        };
    }

    private static string? Trimmed(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static decimal? SanitiseWeight(decimal? value)
    {
        if (value is not { } weight)
        {
            return null;
        }

        return weight < 0 ? 0 : Math.Round(weight, 2);
    }

    private static decimal? SanitiseYears(decimal? value)
    {
        if (value is not { } years)
        {
            return null;
        }

        return years < 0 ? 0 : Math.Round(years, 1);
    }

    private static string NormaliseRecordType(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "LIFTING_CARRYING" : value.Trim().ToUpperInvariant();

    private static bool IsValidRecordType(string value) => GetAllowedRecordTypes().Contains(value);

    private static string[] GetAllowedRecordTypes() => new[]
    {
        "LIFTING_CARRYING",
        "STAIRS_LADDERS_RUNGS",
        "KNEELING_SQUATTING",
        "NECK_SHOULDER_CARRIAGE",
        "HEAVY_LOAD_CARRYING",
        "HAZARD_EXPOSURE"
    };

    private static string NormaliseBodyArea(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value) ? "MULTIPLE" : value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "LUMBAR_SPINE" or "CERVICAL_SPINE" or "KNEES" or "SHOULDERS" or "HIPS" or "MULTIPLE" or "OTHER" => normalised,
            _ => "OTHER"
        };
    }

    private static string NormaliseFrequency(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "DAILY" or "MOST_DAYS" or "WEEKLY" or "MONTHLY" or "OCCASIONAL" or "UNSURE" => normalised,
            _ => "UNSURE"
        };
    }

    private static string NormaliseHazardType(string? value)
    {
        var normalised = string.IsNullOrWhiteSpace(value) ? "OTHER" : value.Trim().ToUpperInvariant();

        return normalised switch
        {
            "WHOLE_BODY_VIBRATION" or "AWKWARD_SUSTAINED_POSTURE" or "SUDDEN_UNEXPECTED_LOAD"
                or "SLIP_TRIP_FALL" or "CONFINED_SPACE" or "REPETITIVE_STRAIN" or "OTHER" => normalised,
            _ => "OTHER"
        };
    }
}

public sealed record CreateLoadExposureRecordRequest(
    Guid? ConditionId,
    string? RecordType,
    string? ActivityDescription,
    string? BodyAreaAffected,
    decimal? TypicalWeightKg,
    decimal? MaxWeightKg,
    string? Frequency,
    string? DurationPerOccasion,
    string? RepetitionsDescription,
    DateOnly? ServicePeriodFrom,
    DateOnly? ServicePeriodTo,
    decimal? YearsExposed,
    string? EquipmentOrContext,
    string? HazardType,
    string? Notes
);

public sealed record UpdateLoadExposureRecordRequest(
    string? RecordType,
    string? ActivityDescription,
    string? BodyAreaAffected,
    decimal? TypicalWeightKg,
    decimal? MaxWeightKg,
    string? Frequency,
    string? DurationPerOccasion,
    string? RepetitionsDescription,
    DateOnly? ServicePeriodFrom,
    DateOnly? ServicePeriodTo,
    decimal? YearsExposed,
    string? EquipmentOrContext,
    string? HazardType,
    string? Notes
);
