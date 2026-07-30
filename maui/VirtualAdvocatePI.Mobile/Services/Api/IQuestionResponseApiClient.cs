using VirtualAdvocatePI.Mobile.Models.QuestionResponses;

namespace VirtualAdvocatePI.Mobile.Services.Api;

public interface IQuestionResponseApiClient
{
    Task<IReadOnlyList<QuestionResponse>> GetQuestionResponsesAsync(
        Guid workspaceId,
        Guid conditionId,
        CancellationToken cancellationToken = default);

    Task<QuestionResponse> CreateQuestionResponseAsync(
        Guid workspaceId,
        Guid conditionId,
        CreateQuestionResponseRequest request,
        CancellationToken cancellationToken = default);
}
