using Microsoft.EntityFrameworkCore;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly SlideBuilderDbContext _context;

    public ConversationRepository(SlideBuilderDbContext context)
    {
        _context = context;
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(
        Guid projectId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ConversationMessage>> GetRecentMessagesAsync(
        Guid projectId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .Where(m => m.ProjectId == projectId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationMessage> AddMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);
        return message;
    }

    public async Task<List<ConversationSummary>> GetSummariesAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConversationSummaries
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.CoveringMessagesFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<ConversationSummary> AddSummaryAsync(
        ConversationSummary summary,
        CancellationToken cancellationToken = default)
    {
        _context.ConversationSummaries.Add(summary);
        await _context.SaveChangesAsync(cancellationToken);
        return summary;
    }
}
