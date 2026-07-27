using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;
using VirtualAdvocatePI.Mobile.Services.Api;

namespace VirtualAdvocatePI.Mobile.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IClaimWorkspaceApiClient _claimWorkspaceApiClient;

    public DashboardService(IClaimWorkspaceApiClient claimWorkspaceApiClient)
    {
        _claimWorkspaceApiClient = claimWorkspaceApiClient;
    }

    public Task<IReadOnlyList<ClaimWorkspace>> GetWorkspacesAsync(CancellationToken cancellationToken = default)
    {
        return _claimWorkspaceApiClient.GetClaimWorkspacesAsync(cancellationToken);
    }
}
