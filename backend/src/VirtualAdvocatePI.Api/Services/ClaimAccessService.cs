using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;

namespace VirtualAdvocatePI.Api.Services;

public sealed class ClaimAccessService
{
    private readonly VirtualAdvocateDbContext _db;

    public ClaimAccessService(VirtualAdvocateDbContext db)
    {
        _db = db;
    }

    public async Task<bool> UserOwnsWorkspaceAsync(Guid userId, Guid workspaceId)
    {
        return await _db.ClaimWorkspaces.AnyAsync(x =>
            x.Id == workspaceId &&
            x.UserId == userId &&
            x.Status != "ARCHIVED");
    }

    public async Task<bool> UserOwnsConditionAsync(Guid userId, Guid workspaceId, Guid conditionId)
    {
        var ownsWorkspace = await UserOwnsWorkspaceAsync(userId, workspaceId);

        if (!ownsWorkspace)
        {
            return false;
        }

        return await _db.ClaimConditions.AnyAsync(x =>
            x.Id == conditionId &&
            x.ClaimWorkspaceId == workspaceId &&
            x.Status != "ARCHIVED");
    }
}
