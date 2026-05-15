using VirtualAdvocatePI.Api.Domain.Users;

namespace VirtualAdvocatePI.Api.Services;

public sealed class AdminAccessService
{
    public AdminAccessResult GetAccess(AppUser? user)
    {
        if (user is null)
        {
            return new AdminAccessResult(
                IsAuthenticated: false,
                IsAdmin: false,
                Role: null,
                AccountStatus: null,
                Reason: "Not authenticated");
        }

        if (!string.Equals(user.AccountStatus, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            return new AdminAccessResult(
                IsAuthenticated: true,
                IsAdmin: false,
                Role: user.Role,
                AccountStatus: user.AccountStatus,
                Reason: "Account is not active");
        }

        if (!IsAdminRole(user.Role))
        {
            return new AdminAccessResult(
                IsAuthenticated: true,
                IsAdmin: false,
                Role: user.Role,
                AccountStatus: user.AccountStatus,
                Reason: "User is not an admin");
        }

        return new AdminAccessResult(
            IsAuthenticated: true,
            IsAdmin: true,
            Role: user.Role,
            AccountStatus: user.AccountStatus,
            Reason: "Admin access granted");
    }

    public bool IsAdmin(AppUser? user)
    {
        return GetAccess(user).IsAdmin;
    }

    public static bool IsAdminRole(string? role)
    {
        return string.Equals(role, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(role, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed record AdminAccessResult(
    bool IsAuthenticated,
    bool IsAdmin,
    string? Role,
    string? AccountStatus,
    string Reason
);