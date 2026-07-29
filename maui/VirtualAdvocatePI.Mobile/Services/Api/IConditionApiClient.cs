using VirtualAdvocatePI.Mobile.Models.Conditions;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IConditionApiClient
{
    Task<IReadOnlyList<ClaimCondition>> GetConditionsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    Task<ClaimCondition> CreateConditionAsync(
        Guid workspaceId,
        CreateConditionRequest request,
        CancellationToken cancellationToken = default);

    Task ArchiveConditionAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);
}
