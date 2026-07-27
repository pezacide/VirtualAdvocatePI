using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class ConditionEndpoints
{
    public static IEndpointRouteBuilder MapConditionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions", async (
            Guid workspaceId,
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

            var conditions = await db.ClaimConditions
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED")
                .OrderByDescending(x => x.IsPrimaryCondition)
                .ThenBy(x => x.ConditionName)
                .ToListAsync();

            return Results.Ok(conditions.Select(ToConditionResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions", async (
            Guid workspaceId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateConditionRequest input) =>
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

            if (string.IsNullOrWhiteSpace(input.ConditionName))
            {
                return Results.BadRequest(new { error = "Condition name is required." });
            }

            var diagnosisStatus = NormaliseDiagnosisStatus(input.DiagnosisStatus);

            if (!IsValidDiagnosisStatus(diagnosisStatus))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid diagnosis status.",
                    allowedValues = GetAllowedDiagnosisStatuses()
                });
            }

            var condition = new ClaimCondition
            {
                ClaimWorkspaceId = workspaceId,
                ConditionName = input.ConditionName.Trim(),
                DiagnosisStatus = diagnosisStatus,
                DateDiagnosed = input.DateDiagnosed,
                CurrentSymptoms = input.CurrentSymptoms,
                TreatmentSummary = input.TreatmentSummary,
                MedicationSummary = input.MedicationSummary,
                MedicationSideEffects = input.MedicationSideEffects,
                FunctionalImpactSummary = input.FunctionalImpactSummary,
                LifestyleImpactSummary = input.LifestyleImpactSummary,
                WorkImpactSummary = input.WorkImpactSummary,
                StabilityNotes = input.StabilityNotes,
                WorseningNotes = input.WorseningNotes,
                IsPrimaryCondition = input.IsPrimaryCondition ?? true,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.ClaimConditions.Add(condition);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "CONDITION_CREATED",
                $"Condition created. ConditionName={condition.ConditionName}; ConditionId={condition.Id}");

            await db.SaveChangesAsync();

            return Results.Created($"/api/v1/claim-workspaces/{workspaceId}/conditions/{condition.Id}", ToConditionResponse(condition));
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
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

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var condition = await db.ClaimConditions
                .FirstOrDefaultAsync(x => x.Id == conditionId && x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED");

            if (condition is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToConditionResponse(condition));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateConditionRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var condition = await db.ClaimConditions
                .FirstOrDefaultAsync(x => x.Id == conditionId && x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED");

            if (condition is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.ConditionName))
            {
                condition.ConditionName = input.ConditionName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(input.DiagnosisStatus))
            {
                var diagnosisStatus = NormaliseDiagnosisStatus(input.DiagnosisStatus);

                if (!IsValidDiagnosisStatus(diagnosisStatus))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid diagnosis status.",
                        allowedValues = GetAllowedDiagnosisStatuses()
                    });
                }

                condition.DiagnosisStatus = diagnosisStatus;
            }

            condition.DateDiagnosed = input.DateDiagnosed ?? condition.DateDiagnosed;
            condition.CurrentSymptoms = input.CurrentSymptoms ?? condition.CurrentSymptoms;
            condition.TreatmentSummary = input.TreatmentSummary ?? condition.TreatmentSummary;
            condition.MedicationSummary = input.MedicationSummary ?? condition.MedicationSummary;
            condition.MedicationSideEffects = input.MedicationSideEffects ?? condition.MedicationSideEffects;
            condition.FunctionalImpactSummary = input.FunctionalImpactSummary ?? condition.FunctionalImpactSummary;
            condition.LifestyleImpactSummary = input.LifestyleImpactSummary ?? condition.LifestyleImpactSummary;
            condition.WorkImpactSummary = input.WorkImpactSummary ?? condition.WorkImpactSummary;
            condition.StabilityNotes = input.StabilityNotes ?? condition.StabilityNotes;
            condition.WorseningNotes = input.WorseningNotes ?? condition.WorseningNotes;

            if (input.IsPrimaryCondition.HasValue)
            {
                condition.IsPrimaryCondition = input.IsPrimaryCondition.Value;
            }

            condition.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "CONDITION_UPDATED",
                $"Condition updated. ConditionName={condition.ConditionName}; ConditionId={condition.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToConditionResponse(condition));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
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

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var condition = await db.ClaimConditions
                .FirstOrDefaultAsync(x => x.Id == conditionId && x.ClaimWorkspaceId == workspaceId && x.Status != "ARCHIVED");

            if (condition is null)
            {
                return Results.NotFound();
            }

            condition.Status = "ARCHIVED";
            condition.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "CONDITION_ARCHIVED",
                $"Condition archived. ConditionName={condition.ConditionName}; ConditionId={condition.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = condition.Id,
                status = condition.Status,
                archived = true
            });
        });

        return app;
    }

    internal static object ToConditionResponse(ClaimCondition condition)
    {
        return new
        {
            id = condition.Id,
            claimWorkspaceId = condition.ClaimWorkspaceId,
            conditionName = condition.ConditionName,
            diagnosisStatus = condition.DiagnosisStatus,
            dateDiagnosed = condition.DateDiagnosed,
            currentSymptoms = condition.CurrentSymptoms,
            treatmentSummary = condition.TreatmentSummary,
            medicationSummary = condition.MedicationSummary,
            medicationSideEffects = condition.MedicationSideEffects,
            functionalImpactSummary = condition.FunctionalImpactSummary,
            lifestyleImpactSummary = condition.LifestyleImpactSummary,
            workImpactSummary = condition.WorkImpactSummary,
            stabilityNotes = condition.StabilityNotes,
            worseningNotes = condition.WorseningNotes,
            isPrimaryCondition = condition.IsPrimaryCondition,
            status = condition.Status,
            createdAt = condition.CreatedAt,
            updatedAt = condition.UpdatedAt
        };
    }

    private static string NormaliseDiagnosisStatus(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidDiagnosisStatus(string value) => GetAllowedDiagnosisStatuses().Contains(value);

    private static string[] GetAllowedDiagnosisStatuses()
    {
        return new[]
        {
            "DIAGNOSED",
            "SUSPECTED",
            "UNSURE",
            "NOT_DIAGNOSED"
        };
    }
}

public sealed record CreateConditionRequest(
    string? ConditionName,
    string? DiagnosisStatus,
    DateOnly? DateDiagnosed,
    string? CurrentSymptoms,
    string? TreatmentSummary,
    string? MedicationSummary,
    string? MedicationSideEffects,
    string? FunctionalImpactSummary,
    string? LifestyleImpactSummary,
    string? WorkImpactSummary,
    string? StabilityNotes,
    string? WorseningNotes,
    bool? IsPrimaryCondition
);

public sealed record UpdateConditionRequest(
    string? ConditionName,
    string? DiagnosisStatus,
    DateOnly? DateDiagnosed,
    string? CurrentSymptoms,
    string? TreatmentSummary,
    string? MedicationSummary,
    string? MedicationSideEffects,
    string? FunctionalImpactSummary,
    string? LifestyleImpactSummary,
    string? WorkImpactSummary,
    string? StabilityNotes,
    string? WorseningNotes,
    bool? IsPrimaryCondition
);
