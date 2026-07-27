using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;

namespace VirtualAdvocatePI.Mobile.Services.Dashboard;

public interface IDashboardService
{
    Task<IReadOnlyList<ClaimWorkspace>> GetWorkspacesAsync(CancellationToken cancellationToken = default);
}
