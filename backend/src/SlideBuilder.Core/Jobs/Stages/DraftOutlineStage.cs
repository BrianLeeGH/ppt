using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.AI;
using SlideBuilder.Core.AI.Models;
using SlideBuilder.Core.AI.Prompts;
using SlideBuilder.Core.Events;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SlideBuilder.Core.Jobs.Stages;

public class DraftOutlineStage : IJobStage
{
    public string Name => JobStages.DraftOutline;

    private readonly IUnitOfWork _uow;
    private readonly IConversationRepository _conversationRepo;
    private readonly IModelClient _modelClient;
    private readonly ContextWindowManager _contextManager;
    private readonly OutlinePromptBuilder _promptBuilder;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DraftOutlineStage> _logger;

    public DraftOutlineStage(
        IUnitOfWork uow,
        IConversationRepository conversationRepo,
        IModelClient modelClient,
        ContextWindowManager contextManager,
        OutlinePromptBuilder promptBuilder,
        IEventPublisher eventPublisher,
        ILogger<DraftOutlineStage> logger)
    {
        _uow = uow;
        _conversationRepo = conversationRepo;
        _modelClient = modelClient;
        _contextManager = contextManager;
        _promptBuilder = promptBuilder;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken ct)
    {
        _logger.LogInformation("Starting DraftOutline stage for job {JobId}", job.Id);

        try
        {
            // Load project and deck
            var project = await _uow.Projects.GetByIdAsync(job.ProjectId);
            if (project == null)
            {
                throw new InvalidOperationException($"Project {job.ProjectId} not found");
            }

            // Ensure deck exists
            var deck = project.Deck;
            if (deck == null)
            {
                deck = new Deck
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id
                };
                project.Deck = deck;
                await _uow.SaveChangesAsync(ct);
            }

            // Get conversation messages
            var messages = await _conversationRepo.GetMessagesAsync(project.Id, 100, ct);
            if (!messages.Any())
            {
                _logger.LogWarning("No conversation messages found for project {ProjectId}", project.Id);
                return;
            }

            // Get the last user message
            var lastUserMessage = messages.LastOrDefault(m => m.Role == "User");
            if (lastUserMessage == null)
            {
                _logger.LogWarning("No user message found in conversation");
                return;
            }

            // Get current outline if it exists
            var currentOutline = deck.DraftOutline;

            // Determine request type and build appropriate prompt
            string prompt;
            bool isApproval = _promptBuilder.IsApprovalRequest(lastUserMessage.Content);
            bool isEdit = _promptBuilder.IsEditRequest(lastUserMessage.Content, currentOutline);

            if (isApproval && currentOutline != null)
            {
                // Handle approval
                await HandleApprovalAsync(project, deck, currentOutline, lastUserMessage, ct);
                return;
            }
            else if (isEdit && currentOutline != null)
            {
                // Edit request
                prompt = _promptBuilder.BuildEditRequestPrompt(lastUserMessage.Content, currentOutline);
            }
            else
            {
                // Initial generation
                prompt = _promptBuilder.BuildInitialGenerationPrompt(lastUserMessage.Content, currentOutline);
            }

            _logger.LogInformation("Calling AI model for outline generation");

            // Call AI model
            var proposal = await _modelClient.GenerateStructuredAsync<OutlineProposal>(prompt, ct);

            // Check if clarification is needed
            if (proposal.NeedsClarification && proposal.ClarificationQuestions != null && proposal.ClarificationQuestions.Any())
            {
                // Create assistant message with clarification questions
                var clarificationMessage = new ConversationMessage
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Role = "Assistant",
                    Content = string.Join("\n", proposal.ClarificationQuestions),
                    CreatedAt = DateTime.UtcNow
                };

                await _conversationRepo.AddMessageAsync(clarificationMessage, ct);

                // Notify via SignalR
                await _eventPublisher.PublishMessageReceivedAsync(
                    new MessageReceivedEvent(
                        project.Id,
                        new
                        {
                            id = clarificationMessage.Id,
                            role = clarificationMessage.Role,
                            content = clarificationMessage.Content,
                            createdAt = clarificationMessage.CreatedAt
                        }
                    ), ct);

                _logger.LogInformation("Clarification requested for job {JobId}", job.Id);
                return;
            }

            // Create or update outline
            if (currentOutline == null)
            {
                currentOutline = new Outline
                {
                    Id = Guid.NewGuid(),
                    DeckId = deck.Id,
                    Status = OutlineStatus.Draft,
                    SlidesJson = JsonSerializer.Serialize(proposal.Slides),
                    CreatedAt = DateTime.UtcNow
                };
                deck.DraftOutline = currentOutline;
            }
            else
            {
                currentOutline.SlidesJson = JsonSerializer.Serialize(proposal.Slides);
            }

            await _uow.SaveChangesAsync(ct);

            // Create outline revision
            var revisionNumber = await GetNextRevisionNumberAsync(currentOutline.Id, ct);
            var revision = new OutlineRevision
            {
                Id = Guid.NewGuid(),
                OutlineId = currentOutline.Id,
                TriggeredByMessageId = lastUserMessage.Id,
                SlidesJson = currentOutline.SlidesJson,
                RevisionNumber = revisionNumber,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.OutlineRevisions.AddAsync(revision);
            await _uow.SaveChangesAsync(ct);

            // Create assistant confirmation message
            var confirmationMessage = new ConversationMessage
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Role = "Assistant",
                Content = isEdit
                    ? "I've updated the outline based on your request."
                    : $"I've created an outline with {proposal.Slides.Count} slides. You can review and ask for changes, or approve it when ready.",
                CreatedAt = DateTime.UtcNow
            };

            await _conversationRepo.AddMessageAsync(confirmationMessage, ct);

            // Notify via SignalR - Outline Updated
            await _eventPublisher.PublishOutlineUpdatedAsync(
                new OutlineUpdatedEvent(
                    project.Id,
                    currentOutline.Id,
                    new
                    {
                        id = currentOutline.Id,
                        deckId = currentOutline.DeckId,
                        status = currentOutline.Status.ToString(),
                        slides = proposal.Slides.Select(s => new
                        {
                            title = s.Title,
                            keyPoints = s.KeyPoints
                        }).ToList(),
                        createdAt = currentOutline.CreatedAt
                    },
                    revisionNumber,
                    Enumerable.Range(0, proposal.Slides.Count).ToList()
                ), ct);

            // Notify via SignalR - Message Received
            await _eventPublisher.PublishMessageReceivedAsync(
                new MessageReceivedEvent(
                    project.Id,
                    new
                    {
                        id = confirmationMessage.Id,
                        role = confirmationMessage.Role,
                        content = confirmationMessage.Content,
                        createdAt = confirmationMessage.CreatedAt
                    }
                ), ct);

            _logger.LogInformation("Completed DraftOutline stage for job {JobId}", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DraftOutline stage for job {JobId}", job.Id);
            throw;
        }
    }

    private async Task HandleApprovalAsync(
        Project project,
        Deck deck,
        Outline outline,
        ConversationMessage triggeringMessage,
        CancellationToken ct)
    {
        _logger.LogInformation("Handling outline approval for project {ProjectId}", project.Id);

        // Update outline status to Approved
        outline.Status = OutlineStatus.Approved;
        await _uow.SaveChangesAsync(ct);

        // Create revision for approval
        var revisionNumber = await GetNextRevisionNumberAsync(outline.Id, ct);
        var revision = new OutlineRevision
        {
            Id = Guid.NewGuid(),
            OutlineId = outline.Id,
            TriggeredByMessageId = triggeringMessage.Id,
            SlidesJson = outline.SlidesJson,
            RevisionNumber = revisionNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.OutlineRevisions.AddAsync(revision);
        await _uow.SaveChangesAsync(ct);

        // Create confirmation message
        var confirmationMessage = new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            Role = "System",
            Content = "✓ Outline approved! The outline is now locked for slide generation. To make changes, I'll create a new draft version while preserving this approved outline.",
            CreatedAt = DateTime.UtcNow
        };

        await _conversationRepo.AddMessageAsync(confirmationMessage, ct);

        // Notify via SignalR
        var slides = JsonSerializer.Deserialize<List<SlideProposal>>(outline.SlidesJson) ?? new List<SlideProposal>();

        await _eventPublisher.PublishOutlineUpdatedAsync(
            new OutlineUpdatedEvent(
                project.Id,
                outline.Id,
                new
                {
                    id = outline.Id,
                    deckId = outline.DeckId,
                    status = outline.Status.ToString(),
                    slides = slides.Select(s => new
                    {
                        title = s.Title,
                        keyPoints = s.KeyPoints
                    }).ToList(),
                    createdAt = outline.CreatedAt
                },
                revisionNumber,
                new List<int>()
            ), ct);

        await _eventPublisher.PublishMessageReceivedAsync(
            new MessageReceivedEvent(
                project.Id,
                new
                {
                    id = confirmationMessage.Id,
                    role = confirmationMessage.Role,
                    content = confirmationMessage.Content,
                    createdAt = confirmationMessage.CreatedAt
                }
            ), ct);
    }

    private async Task<int> GetNextRevisionNumberAsync(Guid outlineId, CancellationToken ct)
    {
        var revisions = await _uow.OutlineRevisions.GetByOutlineIdAsync(outlineId, ct);
        return revisions.Any() ? revisions.Max(r => r.RevisionNumber) + 1 : 1;
    }
}
