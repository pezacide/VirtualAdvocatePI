using Microsoft.EntityFrameworkCore;
using Npgsql;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Users;
using VirtualAdvocatePI.Api.Features.ClaimWorkspaces;
using VirtualAdvocatePI.Api.Features.Users;
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

builder.Services.AddSingleton<IFirebaseAuthService, FirebaseAuthService>();
builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddScoped<ClaimAccessService>();
builder.Services.AddScoped<AdminAccessService>();
builder.Services.AddScoped<AuditService>();

var allowedWebOrigins = (
        Environment.GetEnvironmentVariable("ALLOWED_WEB_ORIGINS")
        ?? builder.Configuration["Cors:AllowedOrigins"]
        ?? "http://localhost:3000,https://localhost:3000")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebAppCors", policy =>
    {
        policy
            .WithOrigins(allowedWebOrigins)
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
    phase = "Phase 11 - Android and iOS app MVP",
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

app.MapDisclaimerAcceptanceEndpoints();

app.MapClaimWorkspaceEndpoints();
app.MapConditionEndpoints();
app.MapAcceptedConditionHistoryEndpoints();
app.MapQuestionResponseEndpoints();

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

public partial class Program { }
