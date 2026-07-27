using VirtualAdvocatePI.Mobile.Models.ClaimWorkspaces;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IClaimWorkspaceApiClient
{
    Task<IReadOnlyList<ClaimWorkspace>> GetClaimWorkspacesAsync(CancellationToken cancellationToken = default);

    Task<ClaimWorkspace> GetClaimWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    Task<ClaimWorkspace> CreateClaimWorkspaceAsync(
        CreateClaimWorkspaceRequest request,
        CancellationToken cancellationToken = default);
}
