using Microsoft.EntityFrameworkCore;
using Npgsql;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Domain.Users;

var builder = WebApplication.CreateBuilder(args);

var databaseConnectionString =
    Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<VirtualAdvocateDbContext>(options =>
{
    options.UseNpgsql(databaseConnectionString);
});

builder.Services.AddSingleton<FirebaseAuthService>();

var app = builder.Build();

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
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(ToUserResponse(user));
});

app.MapGet("/api/v1/claim-workspaces", async (
    HttpRequest request,
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var workspaceRows = await db.ClaimWorkspaces
        .Where(x => x.UserId == user.Id && x.Status != "ARCHIVED")
        .OrderByDescending(x => x.UpdatedAt)
        .ToListAsync();

    var workspaces = workspaceRows
        .Select(ToClaimWorkspaceResponse)
        .ToList();

    return Results.Ok(workspaces);
});

app.MapPost("/api/v1/claim-workspaces", async (
    HttpRequest request,
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db,
    CreateClaimWorkspaceRequest input) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

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

    var title = string.IsNullOrWhiteSpace(input.WorkspaceTitle)
        ? "Post-2026 PI Claim Starter Pack"
        : input.WorkspaceTitle.Trim();

    var workspace = new ClaimWorkspace
    {
        UserId = user.Id,
        ClaimFramework = "IMPROVED_MRCA_POST_2026",
        ClaimScenario = claimScenario,
        WorkspaceTitle = title,
        Status = "IN_PROGRESS",
        GeneratedPackStatus = "NOT_GENERATED",
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow,
        LastOpenedAt = DateTimeOffset.UtcNow
    };

    db.ClaimWorkspaces.Add(workspace);

    await db.SaveChangesAsync();

    return Results.Created($"/api/v1/claim-workspaces/{workspace.Id}", ToClaimWorkspaceResponse(workspace));
});

app.MapGet("/api/v1/claim-workspaces/{id:guid}", async (
    Guid id,
    HttpRequest request,
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && x.Status != "ARCHIVED");

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
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db,
    UpdateClaimWorkspaceRequest input) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && x.Status != "ARCHIVED");

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

    await db.SaveChangesAsync();

    return Results.Ok(ToClaimWorkspaceResponse(workspace));
});

app.MapDelete("/api/v1/claim-workspaces/{id:guid}", async (
    Guid id,
    HttpRequest request,
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db) =>
{
    var user = await GetOrCreateCurrentUserAsync(request, firebaseAuthService, db);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    var workspace = await db.ClaimWorkspaces
        .FirstOrDefaultAsync(x => x.Id == id && x.UserId == user.Id && x.Status != "ARCHIVED");

    if (workspace is null)
    {
        return Results.NotFound();
    }

    workspace.Status = "ARCHIVED";
    workspace.UpdatedAt = DateTimeOffset.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        id = workspace.Id,
        status = workspace.Status,
        archived = true
    });
});

app.Run();

static async Task<AppUser?> GetOrCreateCurrentUserAsync(
    HttpRequest request,
    FirebaseAuthService firebaseAuthService,
    VirtualAdvocateDbContext db)
{
    AuthenticatedFirebaseUser? firebaseUser;

    try
    {
        firebaseUser = await firebaseAuthService.VerifyBearerTokenAsync(request);
    }
    catch
    {
        return null;
    }

    if (firebaseUser is null)
    {
        return null;
    }

    var email = firebaseUser.Email ?? string.Empty;

    var user = await db.Users
        .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUser.FirebaseUid);

    if (user is null)
    {
        user = new AppUser
        {
            FirebaseUid = firebaseUser.FirebaseUid,
            Email = email,
            DisplayName = firebaseUser.DisplayName,
            Role = "VETERAN",
            AccountStatus = "ACTIVE",
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        db.Users.Add(user);
    }
    else
    {
        user.Email = email;
        user.DisplayName = firebaseUser.DisplayName;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        user.AccountStatus = "ACTIVE";
    }

    await db.SaveChangesAsync();

    return user;
}

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

static string NormaliseClaimScenario(string? claimScenario)
{
    if (string.IsNullOrWhiteSpace(claimScenario))
    {
        return "UNSURE";
    }

    return claimScenario.Trim().ToUpperInvariant();
}

static bool IsValidClaimScenario(string claimScenario)
{
    return GetAllowedClaimScenarios().Contains(claimScenario);
}

static bool IsValidWorkspaceStatus(string status)
{
    return GetAllowedWorkspaceStatuses().Contains(status);
}

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

public sealed record CreateClaimWorkspaceRequest(
    string? ClaimScenario,
    string? WorkspaceTitle
);

public sealed record UpdateClaimWorkspaceRequest(
    string? ClaimScenario,
    string? WorkspaceTitle,
    string? Status
);
