using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.Persistence;

public interface IConversationRepository
{
    Task<List<ConversationMessage>> GetMessagesAsync(Guid projectId, int limit = 50, CancellationToken cancellationToken = default);
    Task<List<ConversationMessage>> GetRecentMessagesAsync(Guid projectId, int count, CancellationToken cancellationToken = default);
    Task<ConversationMessage> AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
    Task<List<ConversationSummary>> GetSummariesAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ConversationSummary> AddSummaryAsync(ConversationSummary summary, CancellationToken cancellationToken = default);
}
