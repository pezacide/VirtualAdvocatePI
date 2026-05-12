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

        var user = await _db.Users
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

            _db.Users.Add(user);
        }
        else
        {
            user.Email = email;
            user.DisplayName = firebaseUser.DisplayName;
            user.LastLoginAt = DateTimeOffset.UtcNow;
            user.AccountStatus = "ACTIVE";
        }

        await _db.SaveChangesAsync();

        return user;
    }
}
