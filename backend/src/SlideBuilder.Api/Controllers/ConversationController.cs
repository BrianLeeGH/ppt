using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Jobs;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/conversation")]
public class ConversationController : ControllerBase
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobRunner _jobRunner;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(
        IConversationRepository conversationRepo,
        IUnitOfWork unitOfWork,
        IJobRunner jobRunner,
        ILogger<ConversationController> logger)
    {
        _conversationRepo = conversationRepo;
        _unitOfWork = unitOfWork;
        _jobRunner = jobRunner;
        _logger = logger;
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetMessages(
        Guid projectId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting messages for project {ProjectId}, limit {Limit}", projectId, limit);

        var messages = await _conversationRepo.GetMessagesAsync(projectId, limit, cancellationToken);

        var messageDtos = messages.Select(m => new ConversationMessageDto(
            m.Id,
            m.Role,
            m.Content,
            m.CreatedAt
        )).ToList();

        return Ok(new { messages = messageDtos });
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage(
        Guid projectId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(new { error = "Message content cannot be empty" });
        }

        if (request.Content.Length > 10000)
        {
            return BadRequest(new { error = "Message content exceeds maximum length of 10,000 characters" });
        }

        _logger.LogInformation("Sending message for project {ProjectId}", projectId);

        // Check if project exists
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
        if (project == null)
        {
            return NotFound(new { error = "Project not found" });
        }

        // Save user message
        var message = new ConversationMessage
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Role = "User",
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        await _conversationRepo.AddMessageAsync(message, cancellationToken);

        // Create job to process the message
        var job = new Job
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Status = JobStatus.Queued,
            Stage = "DraftOutline",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            InputSummary = $"Process conversation message: {request.Content.Substring(0, Math.Min(50, request.Content.Length))}..."
        };

        await _unitOfWork.Jobs.AddAsync(job);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Queue the job for processing
        _ = Task.Run(async () =>
        {
            try
            {
                await _jobRunner.RunAsync(job.Id, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing conversation job {JobId}", job.Id);
            }
        });

        var messageDto = new ConversationMessageDto(
            message.Id,
            message.Role,
            message.Content,
            message.CreatedAt
        );

        return Ok(new
        {
            message = messageDto,
            jobId = job.Id
        });
    }
}
