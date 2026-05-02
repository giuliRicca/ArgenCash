namespace ArgenCash.Application.Interfaces;

public interface IAssistantService
{
    Task<AssistantDraftResponse> CreateTransactionDraftAsync(Guid userId, TransactionDraftRequest request, CancellationToken cancellationToken = default);
    Task<AssistantChatResponse> HandleChatAsync(Guid userId, AssistantChatRequest request, CancellationToken cancellationToken = default);
}
