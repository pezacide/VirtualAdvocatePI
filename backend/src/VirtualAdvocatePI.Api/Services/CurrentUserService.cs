using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Auth;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Users;

namespace VirtualAdvocatePI.Api.Services;

public sealed class CurrentUserService
{
    private readonly FirebaseAuthService _firebaseAuthService;
    private readonly VirtualAdvocateDbContext _db;

    public CurrentUserService(
        FirebaseAuthService firebaseAuthService,
        VirtualAdvocateDbContext db)
    {
        _firebaseAuthService = firebaseAuthService;
        _db = db;
    }

    public async Task<AppUser?> GetOrCreateCurrentUserAsync(HttpRequest request)
    {
        AuthenticatedFirebaseUser? firebaseUser;

        try
        {
            firebaseUser = await _firebaseAuthService.VerifyBearerTokenAsync(request);
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
        var isConfiguredAdmin = IsConfiguredAdminEmail(email);

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUser.FirebaseUid);

        if (user is null)
        {
            user = new AppUser
            {
                FirebaseUid = firebaseUser.FirebaseUid,
                Email = email,
                DisplayName = firebaseUser.DisplayName,
                Role = isConfiguredAdmin ? "ADMIN" : "VETERAN",
                AccountStatus = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow
            };

            _db.Users.Add(user);
        }
        else
        {
            user.Email = email;
            user.DisplayName = firebaseUser.DisplayName;
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.AccountStatus = "ACTIVE";

            if (isConfiguredAdmin && !IsAdminRole(user.Role))
            {
                user.Role = "ADMIN";
            }
        }

        await _db.SaveChangesAsync();

        return user;
    }

    private static bool IsConfiguredAdminEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var configuredEmails = Environment.GetEnvironmentVariable("VAPI_ADMIN_EMAILS")
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAILS")
            ?? string.Empty;

        return configuredEmails
            .Split(';', ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(configuredEmail => string.Equals(
                configuredEmail,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsAdminRole(string? role)
    {
        return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase);
    }
}