using Microsoft.EntityFrameworkCore;
using Npgsql;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Users;
using VirtualAdvocatePI.Api.Features.Evidence;
using VirtualAdvocatePI.Api.Features.Ai;
using VirtualAdvocatePI.Api.Features.Documents;
using VirtualAdvocatePI.Api.Services;
using VirtualAdvocatePI.Api.Features.Admin;
using VirtualAdvocatePI.Api.Features.Mobile;

var builder = WebApplication.CreateBuilder(args);

var databaseConnectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VirtualAdvocateDbContext>(options =>
{
    options.UseNpgsql(databaseConnectionString);
});

builder.Services.AddSingleton<FirebaseAuthService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ClaimAccessService>();
builder.Services.AddScoped<AdminAccessService>();
builder.Services.AddScoped<AuditService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAppCors", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("WebAppCors");

app.MapGet("/", () => Results.Ok(new
{
    app = "Virtual Advocate PI",
    service = "vapi-dev-api",
    status = "running",
    message = "Virtual Advocate PI API is online"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    service = "vapi-dev-api",
    app = "Virtual Advocate PI"
}));

app.MapGet("/api/v1/build-info", () => Results.Ok(new
{
    app = "Virtual Advocate PI",
    phase = "Phase 3 - Core backend and database",
    framework = "IMPROVED_MRCA_POST_2026",
    purpose = "Post-1 July 2026 PI Claim Starter Pack API"
}));

app.MapGet("/api/v1/config/secret-health", () =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

    return Results.Ok(new
    {
        status = string.IsNullOrWhiteSpace(connectionString) ? "missing" : "present",
        secret = "DATABASE_CONNECTION_STRING",
        valueDisplayed = false
    });
});

app.MapGet("/api/v1/db/health", async () =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem("DATABASE_CONNECTION_STRING environment variable is missing.");
    }

    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    await using var command = new NpgsqlCommand("select current_database(), current_user, version();", connection);
    await using var reader = await command.ExecuteReaderAsync();

    if (!await reader.ReadAsync())
    {
        return Results.Problem("Database query returned no rows.");
    }

    return Results.Ok(new
    {
        status = "connected",
        database = reader.GetString(0),
        user = reader.GetString(1),
        postgresVersion = reader.GetString(2)
    });
});

app.MapGet("/api/v1/db/schema-health", async (VirtualAdvocateDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();

    return Results.Ok(new
    {
        status = canConnect ? "connected" : "not_connected",
        databaseProvider = db.Database.ProviderName,
        app = "Virtual Advocate PI"
    });
});

app.MapGet("/api/v1/me", async (
    HttpRequest request,
    CurrentUserService currentUserService) =>
{
    var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(ToUserResponse(user));
});

app.MapGet("/api/v1/claim-workspaces", async (
    HttpRequest request,
    CurrentUserService currentUserService,
    VirtualAdvocateDbContext db) =>
{
    var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var workspaceRows = await db.ClaimWorkspaces
        .Where(x => x.UserId == user.Id && x.Status != "ARCHIVED")
        .OrderByDescending(x => x.UpdatedAt)
        .ToListAsync();

    return Results.Ok(workspaceRows.Select(ToClaimWorkspaceResponse).ToList());
});

app.MapPost("/api/v1/claim-workspaces", async (
    HttpRequest request,
    CurrentUserService currentUserService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    CreateClaimWorkspaceRequest input) =>
{
    var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var claimScenario = NormaliseClaimScenario(input.ClaimScenario);

    if (!IsValidClaimScenario(claimScenario))
    {
        return Results.BadRequest(new
        {
            error = "Invalid claim scenario.",
            allowedValues = GetAllowedClaimScenarios()
        });
    }

    var workspace = new ClaimWorkspace
    {
        UserId = user.Id,
        ClaimFramework = "IMPROVED_MRCA_POST_2026",
        ClaimScenario = claimScenario,
        WorkspaceTitle = string.IsNullOrWhiteSpace(input.WorkspaceTitle)
            ? "Post-2026 PI Claim Starter Pack"
            : input.WorkspaceTitle.Trim(),
        Status = "IN_PROGRESS",
        GeneratedPackStatus = "NOT_GENERATED",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        LastOpenedAt = DateTimeOffset.UtcNow
    };

    db.ClaimWorkspaces.Add(workspace);

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspace.Id,
        "CLAIM_WORKSPACE_CREATED",
        $"Claim workspace created. Scenario={claimScenario}; WorkspaceId={workspace.Id}");

    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/claim-workspaces/{workspace.Id}", ToClaimWorkspaceResponse(workspace));
});

app.MapGet("/api/v1/claim-workspaces/{id:guid}", async (
    Guid id,
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

    if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
    {
        return Results.NotFound();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

    if (workspace is null)
    {
        return Results.NotFound();
    }

    workspace.LastOpenedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(ToClaimWorkspaceResponse(workspace));
});

app.MapPatch("/api/v1/claim-workspaces/{id:guid}", async (
    Guid id,
    HttpRequest request,
    CurrentUserService currentUserService,
    ClaimAccessService claimAccessService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    UpdateClaimWorkspaceRequest input) =>
{
    var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
    {
        return Results.NotFound();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

    if (workspace is null)
    {
        return Results.NotFound();
    }

    if (!string.IsNullOrWhiteSpace(input.WorkspaceTitle))
    {
        workspace.WorkspaceTitle = input.WorkspaceTitle.Trim();
    }

    if (!string.IsNullOrWhiteSpace(input.ClaimScenario))
    {
        var claimScenario = NormaliseClaimScenario(input.ClaimScenario);

        if (!IsValidClaimScenario(claimScenario))
        {
            return Results.BadRequest(new
            {
                error = "Invalid claim scenario.",
                allowedValues = GetAllowedClaimScenarios()
            });
        }

        workspace.ClaimScenario = claimScenario;
    }

    if (!string.IsNullOrWhiteSpace(input.Status))
    {
        var status = input.Status.Trim().ToUpperInvariant();

        if (!IsValidWorkspaceStatus(status))
        {
            return Results.BadRequest(new
            {
                error = "Invalid workspace status.",
                allowedValues = GetAllowedWorkspaceStatuses()
            });
        }

        workspace.Status = status;
    }

    workspace.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspace.Id,
        "CLAIM_WORKSPACE_UPDATED",
        $"Claim workspace updated. Status={workspace.Status}; WorkspaceId={workspace.Id}");

    await db.SaveChangesAsync();

    return Results.Ok(ToClaimWorkspaceResponse(workspace));
});

app.MapDelete("/api/v1/claim-workspaces/{id:guid}", async (
    Guid id,
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

    if (!await claimAccessService.UserOwnsWorkspaceAsync(user.Id, id))
    {
        return Results.NotFound();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.Status != "ARCHIVED");

    if (workspace is null)
    {
        return Results.NotFound();
    }

    workspace.Status = "ARCHIVED";
    workspace.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspace.Id,
        "CLAIM_WORKSPACE_ARCHIVED",
        $"Claim workspace archived. WorkspaceId={workspace.Id}");

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id = workspace.Id,
        status = workspace.Status,
        archived = true
    });
});
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
app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history", async (
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

    var rows = await db.AcceptedConditionHistories
        .Where(x => x.ClaimWorkspaceId == workspaceId && x.ConditionId == conditionId && x.Status != "ARCHIVED")
        .OrderByDescending(x => x.UpdatedAt)
        .ToListAsync();

    return Results.Ok(rows.Select(ToAcceptedHistoryResponse).ToList());
});

app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history", async (
    Guid workspaceId,
    Guid conditionId,
    HttpRequest request,
    CurrentUserService currentUserService,
    ClaimAccessService claimAccessService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    CreateAcceptedHistoryRequest input) =>
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

    var originalAct = NormaliseOriginalAct(input.OriginalAct);

    if (!IsValidOriginalAct(originalAct))
    {
        return Results.BadRequest(new
        {
            error = "Invalid original Act.",
            allowedValues = GetAllowedOriginalActs()
        });
    }

    var history = new AcceptedConditionHistory
    {
        ClaimWorkspaceId = workspaceId,
        ConditionId = conditionId,
        PreviouslyAcceptedByDva = NormaliseYesNoUnsure(input.PreviouslyAcceptedByDva),
        OriginalAct = originalAct,
        PreviousCompensationReceived = NormaliseYesNoUnsure(input.PreviousCompensationReceived),
        PreviousDvaDecisionLetterAvailable = NormaliseYesNoUnsure(input.PreviousDvaDecisionLetterAvailable),
        PreviousAssessmentLetterAvailable = NormaliseYesNoUnsure(input.PreviousAssessmentLetterAvailable),
        PreviousDecisionDate = input.PreviousDecisionDate,
        PreviousAssessmentDate = input.PreviousAssessmentDate,
        WorseningClaimed = NormaliseYesNoUnsure(input.WorseningClaimed),
        WorseningSummary = input.WorseningSummary,
        Status = "ACTIVE",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    db.AcceptedConditionHistories.Add(history);

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "ACCEPTED_CONDITION_HISTORY_CREATED",
        $"Accepted-condition history created. ConditionId={conditionId}; HistoryId={history.Id}; OriginalAct={originalAct}");

    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/accepted-history/{history.Id}", ToAcceptedHistoryResponse(history));
});

app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history/{historyId:guid}", async (
    Guid workspaceId,
    Guid conditionId,
    Guid historyId,
    HttpRequest request,
    CurrentUserService currentUserService,
    ClaimAccessService claimAccessService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    UpdateAcceptedHistoryRequest input) =>
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

    var history = await db.AcceptedConditionHistories
        .FirstOrDefaultAsync(x =>
            x.Id == historyId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.ConditionId == conditionId &&
            x.Status != "ARCHIVED");

    if (history is null)
    {
        return Results.NotFound();
    }

    if (!string.IsNullOrWhiteSpace(input.PreviouslyAcceptedByDva))
    {
        history.PreviouslyAcceptedByDva = NormaliseYesNoUnsure(input.PreviouslyAcceptedByDva);
    }

    if (!string.IsNullOrWhiteSpace(input.OriginalAct))
    {
        var originalAct = NormaliseOriginalAct(input.OriginalAct);

        if (!IsValidOriginalAct(originalAct))
        {
            return Results.BadRequest(new
            {
                error = "Invalid original Act.",
                allowedValues = GetAllowedOriginalActs()
            });
        }

        history.OriginalAct = originalAct;
    }

    if (!string.IsNullOrWhiteSpace(input.PreviousCompensationReceived))
    {
        history.PreviousCompensationReceived = NormaliseYesNoUnsure(input.PreviousCompensationReceived);
    }

    if (!string.IsNullOrWhiteSpace(input.PreviousDvaDecisionLetterAvailable))
    {
        history.PreviousDvaDecisionLetterAvailable = NormaliseYesNoUnsure(input.PreviousDvaDecisionLetterAvailable);
    }

    if (!string.IsNullOrWhiteSpace(input.PreviousAssessmentLetterAvailable))
    {
        history.PreviousAssessmentLetterAvailable = NormaliseYesNoUnsure(input.PreviousAssessmentLetterAvailable);
    }

    if (!string.IsNullOrWhiteSpace(input.WorseningClaimed))
    {
        history.WorseningClaimed = NormaliseYesNoUnsure(input.WorseningClaimed);
    }

    history.PreviousDecisionDate = input.PreviousDecisionDate ?? history.PreviousDecisionDate;
    history.PreviousAssessmentDate = input.PreviousAssessmentDate ?? history.PreviousAssessmentDate;
    history.WorseningSummary = input.WorseningSummary ?? history.WorseningSummary;
    history.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "ACCEPTED_CONDITION_HISTORY_UPDATED",
        $"Accepted-condition history updated. ConditionId={conditionId}; HistoryId={history.Id}; OriginalAct={history.OriginalAct}");

    await db.SaveChangesAsync();

    return Results.Ok(ToAcceptedHistoryResponse(history));
});

app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/accepted-history/{historyId:guid}", async (
    Guid workspaceId,
    Guid conditionId,
    Guid historyId,
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

    var history = await db.AcceptedConditionHistories
        .FirstOrDefaultAsync(x =>
            x.Id == historyId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.ConditionId == conditionId &&
            x.Status != "ARCHIVED");

    if (history is null)
    {
        return Results.NotFound();
    }

    history.Status = "ARCHIVED";
    history.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "ACCEPTED_CONDITION_HISTORY_ARCHIVED",
        $"Accepted-condition history archived. ConditionId={conditionId}; HistoryId={history.Id}");

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id = history.Id,
        status = history.Status,
        archived = true
    });
});
app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses", async (
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

    var responses = await db.QuestionResponses
        .Where(x => x.ClaimWorkspaceId == workspaceId && x.ConditionId == conditionId && x.Status != "ARCHIVED")
        .OrderBy(x => x.QuestionGroup)
        .ThenBy(x => x.QuestionKey)
        .ToListAsync();

    return Results.Ok(responses.Select(ToQuestionResponseResponse).ToList());
});

app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses", async (
    Guid workspaceId,
    Guid conditionId,
    HttpRequest request,
    CurrentUserService currentUserService,
    ClaimAccessService claimAccessService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    CreateQuestionResponseRequest input) =>
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

    if (string.IsNullOrWhiteSpace(input.QuestionKey))
    {
        return Results.BadRequest(new { error = "Question key is required." });
    }

    if (string.IsNullOrWhiteSpace(input.QuestionText))
    {
        return Results.BadRequest(new { error = "Question text is required." });
    }

    var questionGroup = NormaliseQuestionGroup(input.QuestionGroup);

    if (!IsValidQuestionGroup(questionGroup))
    {
        return Results.BadRequest(new
        {
            error = "Invalid question group.",
            allowedValues = GetAllowedQuestionGroups()
        });
    }

    var answerType = NormaliseAnswerType(input.AnswerType);

    if (!IsValidAnswerType(answerType))
    {
        return Results.BadRequest(new
        {
            error = "Invalid answer type.",
            allowedValues = GetAllowedAnswerTypes()
        });
    }

    var response = new QuestionResponse
    {
        ClaimWorkspaceId = workspaceId,
        ConditionId = conditionId,
        QuestionGroup = questionGroup,
        QuestionKey = input.QuestionKey.Trim(),
        QuestionText = input.QuestionText.Trim(),
        AnswerText = input.AnswerText,
        AnswerType = answerType,
        Status = "ACTIVE",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    db.QuestionResponses.Add(response);

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "QUESTION_RESPONSE_CREATED",
        $"Question response created. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

    await db.SaveChangesAsync();

    return Results.Created(
        $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{response.Id}",
        ToQuestionResponseResponse(response));
});

app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
    Guid workspaceId,
    Guid conditionId,
    Guid responseId,
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

    var response = await db.QuestionResponses
        .FirstOrDefaultAsync(x =>
            x.Id == responseId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.ConditionId == conditionId &&
            x.Status != "ARCHIVED");

    if (response is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(ToQuestionResponseResponse(response));
});

app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
    Guid workspaceId,
    Guid conditionId,
    Guid responseId,
    HttpRequest request,
    CurrentUserService currentUserService,
    ClaimAccessService claimAccessService,
    AuditService auditService,
    VirtualAdvocateDbContext db,
    UpdateQuestionResponseRequest input) =>
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

    var response = await db.QuestionResponses
        .FirstOrDefaultAsync(x =>
            x.Id == responseId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.ConditionId == conditionId &&
            x.Status != "ARCHIVED");

    if (response is null)
    {
        return Results.NotFound();
    }

    if (!string.IsNullOrWhiteSpace(input.QuestionGroup))
    {
        var questionGroup = NormaliseQuestionGroup(input.QuestionGroup);

        if (!IsValidQuestionGroup(questionGroup))
        {
            return Results.BadRequest(new
            {
                error = "Invalid question group.",
                allowedValues = GetAllowedQuestionGroups()
            });
        }

        response.QuestionGroup = questionGroup;
    }

    if (!string.IsNullOrWhiteSpace(input.QuestionKey))
    {
        response.QuestionKey = input.QuestionKey.Trim();
    }

    if (!string.IsNullOrWhiteSpace(input.QuestionText))
    {
        response.QuestionText = input.QuestionText.Trim();
    }

    if (input.AnswerText is not null)
    {
        response.AnswerText = input.AnswerText;
    }

    if (!string.IsNullOrWhiteSpace(input.AnswerType))
    {
        var answerType = NormaliseAnswerType(input.AnswerType);

        if (!IsValidAnswerType(answerType))
        {
            return Results.BadRequest(new
            {
                error = "Invalid answer type.",
                allowedValues = GetAllowedAnswerTypes()
            });
        }

        response.AnswerType = answerType;
    }

    response.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "QUESTION_RESPONSE_UPDATED",
        $"Question response updated. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

    await db.SaveChangesAsync();

    return Results.Ok(ToQuestionResponseResponse(response));
});

app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
    Guid workspaceId,
    Guid conditionId,
    Guid responseId,
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

    var response = await db.QuestionResponses
        .FirstOrDefaultAsync(x =>
            x.Id == responseId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.ConditionId == conditionId &&
            x.Status != "ARCHIVED");

    if (response is null)
    {
        return Results.NotFound();
    }

    response.Status = "ARCHIVED";
    response.UpdatedAt = DateTimeOffset.UtcNow;

    auditService.AddAuditEvent(
        request,
        user.Id,
        workspaceId,
        "QUESTION_RESPONSE_ARCHIVED",
        $"Question response archived. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id = response.Id,
        status = response.Status,
        archived = true
    });
});
app.MapEvidenceAndAuditEndpoints();

app.MapEvidenceUploadEndpoints();

app.MapEvidenceGapEndpoints();

app.MapAiDraftEndpoints();
app.MapAiRagRetrievalEndpoints();
app.MapAiDraftRequestEndpoints();
app.MapAiDraftGenerationEndpoints();

app.MapGeneratedDocumentEndpoints();
app.MapClaimStarterPackDocumentEndpoints();
app.MapGeneratedDocumentDownloadEndpoints();
app.MapDoctorGuidancePackDocumentEndpoints();

app.UseMiddleware<AdminAuditLoggingMiddleware>();

app.MapAdminAccessEndpoints();
app.MapAdminSourceRegistryEndpoints();
app.MapAdminSourceRegistrySeedEndpoints();
app.MapAdminTemplateRegistryEndpoints();
app.MapAdminPromptDisclaimerVersionEndpoints();
app.MapAdminKnowledgeAuditEndpoints();
app.MapMobileSessionEndpoints();
app.MapAdminDatabaseMaintenanceEndpoints();

app.Run();

static object ToUserResponse(AppUser user)
{
    return new
    {
        id = user.Id,
        firebaseUid = user.FirebaseUid,
        email = user.Email,
        displayName = user.DisplayName,
        role = user.Role,
        accountStatus = user.AccountStatus
    };
}

static object ToClaimWorkspaceResponse(ClaimWorkspace workspace)
{
    return new
    {
        id = workspace.Id,
        userId = workspace.UserId,
        claimFramework = workspace.ClaimFramework,
        claimScenario = workspace.ClaimScenario,
        workspaceTitle = workspace.WorkspaceTitle,
        status = workspace.Status,
        generatedPackStatus = workspace.GeneratedPackStatus,
        createdAt = workspace.CreatedAt,
        updatedAt = workspace.UpdatedAt,
        lastOpenedAt = workspace.LastOpenedAt
    };
}

static object ToConditionResponse(ClaimCondition condition)
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

static object ToAcceptedHistoryResponse(AcceptedConditionHistory history)
{
    return new
    {
        id = history.Id,
        claimWorkspaceId = history.ClaimWorkspaceId,
        conditionId = history.ConditionId,
        previouslyAcceptedByDva = history.PreviouslyAcceptedByDva,
        originalAct = history.OriginalAct,
        previousCompensationReceived = history.PreviousCompensationReceived,
        previousDvaDecisionLetterAvailable = history.PreviousDvaDecisionLetterAvailable,
        previousAssessmentLetterAvailable = history.PreviousAssessmentLetterAvailable,
        previousDecisionDate = history.PreviousDecisionDate,
        previousAssessmentDate = history.PreviousAssessmentDate,
        worseningClaimed = history.WorseningClaimed,
        worseningSummary = history.WorseningSummary,
        status = history.Status,
        createdAt = history.CreatedAt,
        updatedAt = history.UpdatedAt
    };
}

static object ToQuestionResponseResponse(QuestionResponse response)
{
    return new
    {
        id = response.Id,
        claimWorkspaceId = response.ClaimWorkspaceId,
        conditionId = response.ConditionId,
        questionGroup = response.QuestionGroup,
        questionKey = response.QuestionKey,
        questionText = response.QuestionText,
        answerText = response.AnswerText,
        answerType = response.AnswerType,
        status = response.Status,
        createdAt = response.CreatedAt,
        updatedAt = response.UpdatedAt
    };
}

static string NormaliseClaimScenario(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();
}

static string NormaliseDiagnosisStatus(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "UNSURE" : value.Trim().ToUpperInvariant();
}

static string NormaliseOriginalAct(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "UNKNOWN" : value.Trim().ToUpperInvariant();
}

static string NormaliseYesNoUnsure(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return "UNSURE";
    }

    var normalised = value.Trim().ToUpperInvariant();

    return normalised switch
    {
        "YES" => "YES",
        "NO" => "NO",
        "UNSURE" => "UNSURE",
        "NOT_APPLICABLE" => "NOT_APPLICABLE",
        "N/A" => "NOT_APPLICABLE",
        _ => "UNSURE"
    };
}

static string NormaliseQuestionGroup(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "CLAIM_CONTEXT" : value.Trim().ToUpperInvariant();
}

static string NormaliseAnswerType(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "TEXT" : value.Trim().ToUpperInvariant();
}

static bool IsValidClaimScenario(string value) => GetAllowedClaimScenarios().Contains(value);

static bool IsValidWorkspaceStatus(string value) => GetAllowedWorkspaceStatuses().Contains(value);

static bool IsValidDiagnosisStatus(string value) => GetAllowedDiagnosisStatuses().Contains(value);

static bool IsValidOriginalAct(string value) => GetAllowedOriginalActs().Contains(value);

static bool IsValidQuestionGroup(string value) => GetAllowedQuestionGroups().Contains(value);

static bool IsValidAnswerType(string value) => GetAllowedAnswerTypes().Contains(value);

static string[] GetAllowedClaimScenarios()
{
    return new[]
    {
        "NEW_CONDITION",
        "WORSENING_EXISTING_CONDITION",
        "NEW_PLUS_EXISTING",
        "EVIDENCE_PREP_ONLY",
        "UNSURE"
    };
}

static string[] GetAllowedWorkspaceStatuses()
{
    return new[]
    {
        "NOT_STARTED",
        "IN_PROGRESS",
        "EVIDENCE_GAPS_FOUND",
        "DRAFTS_READY_FOR_REVIEW",
        "READY_TO_GENERATE",
        "PACK_GENERATED",
        "ARCHIVED"
    };
}

static string[] GetAllowedDiagnosisStatuses()
{
    return new[]
    {
        "DIAGNOSED",
        "SUSPECTED",
        "UNSURE",
        "NOT_DIAGNOSED"
    };
}

static string[] GetAllowedOriginalActs()
{
    return new[]
    {
        "VEA",
        "DRCA",
        "MRCA",
        "UNKNOWN",
        "NOT_APPLICABLE"
    };
}

static string[] GetAllowedQuestionGroups()
{
    return new[]
    {
        "CLAIM_CONTEXT",
        "DIAGNOSIS",
        "SYMPTOMS",
        "TREATMENT",
        "MEDICATION",
        "FUNCTIONAL_IMPACT",
        "LIFESTYLE_IMPACT",
        "WORK_IMPACT",
        "STABILITY",
        "WORSENING",
        "PREVIOUS_COMPENSATION",
        "EVIDENCE_AVAILABLE",
        "EVIDENCE_MISSING",
        "DOCTOR_QUESTIONS"
    };
}

static string[] GetAllowedAnswerTypes()
{
    return new[]
    {
        "TEXT",
        "LONG_TEXT",
        "YES_NO",
        "YES_NO_UNSURE",
        "DATE",
        "MULTI_SELECT",
        "SINGLE_SELECT",
        "FILE_REFERENCE"
    };
}

public sealed record CreateClaimWorkspaceRequest(string? ClaimScenario, string? WorkspaceTitle);

public sealed record UpdateClaimWorkspaceRequest(string? ClaimScenario, string? WorkspaceTitle, string? Status);

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

public sealed record CreateAcceptedHistoryRequest(
    string? PreviouslyAcceptedByDva,
    string? OriginalAct,
    string? PreviousCompensationReceived,
    string? PreviousDvaDecisionLetterAvailable,
    string? PreviousAssessmentLetterAvailable,
    DateOnly? PreviousDecisionDate,
    DateOnly? PreviousAssessmentDate,
    string? WorseningClaimed,
    string? WorseningSummary
);

public sealed record UpdateAcceptedHistoryRequest(
    string? PreviouslyAcceptedByDva,
    string? OriginalAct,
    string? PreviousCompensationReceived,
    string? PreviousDvaDecisionLetterAvailable,
    string? PreviousAssessmentLetterAvailable,
    DateOnly? PreviousDecisionDate,
    DateOnly? PreviousAssessmentDate,
    string? WorseningClaimed,
    string? WorseningSummary
);

public sealed record CreateQuestionResponseRequest(
    string? QuestionGroup,
    string? QuestionKey,
    string? QuestionText,
    string? AnswerText,
    string? AnswerType
);

public sealed record UpdateQuestionResponseRequest(
    string? QuestionGroup,
    string? QuestionKey,
    string? QuestionText,
    string? AnswerText,
    string? AnswerType
);






public partial class Program { }

