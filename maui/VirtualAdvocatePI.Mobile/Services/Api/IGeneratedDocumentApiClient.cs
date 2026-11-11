using VirtualAdvocatePI.Mobile.Models.GeneratedDocuments;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IGeneratedDocumentApiClient
{
    Task<IReadOnlyList<GeneratedDocument>> GetGeneratedDocumentsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    Task<GeneratePackResponse> GenerateClaimStarterPackAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    Task<GeneratePackResponse> GenerateDoctorGuidancePackAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    Task<GeneratedDocumentDownloadUrlResponse> CreateDownloadUrlAsync(
        Guid workspaceId,
        Guid documentId,
        string format,
        CancellationToken cancellationToken = default);
}
