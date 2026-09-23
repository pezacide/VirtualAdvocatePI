using VirtualAdvocatePI.Mobile.Models.AiDrafts;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IAiDraftApiClient
{
    Task<IReadOnlyList<AiDraft>> GetConditionAiDraftsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);

    Task<AiDraft> UpdateAiDraftAsync(
        Guid workspaceId,
        Guid draftId,
        UpdateAiDraftRequest request,
        CancellationToken cancellationToken = default);

    Task ArchiveAiDraftAsync(
        Guid workspaceId,
        Guid draftId,
        CancellationToken cancellationToken = default);
}
