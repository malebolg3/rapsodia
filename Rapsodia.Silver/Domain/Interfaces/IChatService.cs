using Rapsodia.Silver.Application.DTOs;
using Rapsodia.Silver.Domain.Common;

namespace Rapsodia.Silver.Application.Interfaces;

public interface IChatService
{
    Task<Result<ChatResponse>> SendMessageAsync(ChatRequest request);
    Task<Result<ConversationSummary>> GetConversationAsync(Guid conversationId);
    Task<PagedResult<ConversationSummary>> ListConversationsAsync(Guid userId, int page = 1, int pageSize = 20);
}