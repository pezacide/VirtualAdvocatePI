using VirtualAdvocatePI.Mobile.Models.Evidence;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IEvidenceApiClient
{
    Task<IReadOnlyList<EvidenceItem>> GetConditionEvidenceItemsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);

    Task<EvidenceItem> CreateEvidenceItemAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateEvidenceItemRequest request,
        CancellationToken cancellationToken = default);

    Task<EvidenceUploadUrlResponse> CreateEvidenceUploadUrlAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateEvidenceUploadUrlRequest request,
        CancellationToken cancellationToken = default);

    Task UploadFileToSignedUrlAsync(
        string signedUrl,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<EvidenceItem> MarkEvidenceUploadedAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default);

    Task<EvidenceDownloadUrlResponse> CreateEvidenceDownloadUrlAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default);

    Task ArchiveEvidenceItemAsync(
        Guid workspaceId,
        Guid evidenceItemId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EvidenceGap>> GetConditionEvidenceGapsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EvidenceGap>> RecalculateEvidenceGapsAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);
}
