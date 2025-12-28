using Microsoft.Extensions.Options;
using SlideBuilder.Core.Configuration;
using SlideBuilder.Core.Domain;
using System.Text;

namespace SlideBuilder.Core.AI.Prompts;

public class ContextWindowManager
{
    private readonly AppSettings _settings;

    public ContextWindowManager(IOptions<AppSettings> settings)
    {
        _settings = settings.Value;
    }

    public string BuildContextForAI(
        List<ConversationMessage> allMessages,
        List<ConversationSummary> summaries,
        Outline? currentOutline)
    {
        var contextBuilder = new StringBuilder();

        // Pin current outline state at the top
        if (currentOutline != null)
        {
            contextBuilder.AppendLine("=== CURRENT OUTLINE STATE ===");
            contextBuilder.AppendLine($"Status: {currentOutline.Status}");
            contextBuilder.AppendLine($"Outline JSON: {currentOutline.SlidesJson}");
            contextBuilder.AppendLine();
        }

        // Add summaries for older messages
        if (summaries.Any())
        {
            contextBuilder.AppendLine("=== CONVERSATION HISTORY SUMMARY ===");
            foreach (var summary in summaries)
            {
                contextBuilder.AppendLine($"Messages {summary.CoveringMessagesFrom}-{summary.CoveringMessagesTo}: {summary.SummaryText}");
            }
            contextBuilder.AppendLine();
        }

        // Determine sliding window size
        var windowSize = _settings.Ai?.ContextWindowSize ?? 20;
        var recentMessages = allMessages.TakeLast(windowSize).ToList();

        // Add recent messages in full
        contextBuilder.AppendLine("=== RECENT CONVERSATION ===");
        foreach (var message in recentMessages)
        {
            contextBuilder.AppendLine($"[{message.Role}]: {message.Content}");
        }

        return contextBuilder.ToString();
    }

    public bool ShouldCreateSummary(int totalMessageCount)
    {
        var threshold = _settings.Ai?.SummaryThreshold ?? 30;
        return totalMessageCount > threshold;
    }

    public async Task<string> SummarizeMessagesAsync(
        List<ConversationMessage> messages,
        IModelClient modelClient,
        CancellationToken cancellationToken = default)
    {
        var messagesText = new StringBuilder();
        messagesText.AppendLine("Summarize the following conversation messages concisely (2-3 sentences):");
        messagesText.AppendLine();

        foreach (var msg in messages)
        {
            messagesText.AppendLine($"[{msg.Role}]: {msg.Content}");
        }

        var summaryPrompt = messagesText.ToString();
        var summary = await modelClient.GenerateAsync(summaryPrompt, cancellationToken);

        return summary.Trim();
    }
}
