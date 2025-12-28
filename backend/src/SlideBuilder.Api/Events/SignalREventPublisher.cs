using Microsoft.AspNetCore.SignalR;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Api.Hubs;
using SlideBuilder.Core.Events;

namespace SlideBuilder.Api.Events;

public class SignalREventPublisher : IEventPublisher
{
    private readonly IHubContext<JobsHub, IJobsClient> _hubContext;

    public SignalREventPublisher(IHubContext<JobsHub, IJobsClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishOutlineUpdatedAsync(
        Core.Events.OutlineUpdatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var dto = new Contracts.OutlineUpdatedEvent(
            @event.ProjectId,
            @event.OutlineId,
            @event.Outline,
            @event.RevisionNumber,
            @event.ChangedSlideIndices
        );

        await _hubContext.Clients
            .Group($"project-{@event.ProjectId}")
            .OutlineUpdated(dto);
    }

    public async Task PublishMessageReceivedAsync(
        Core.Events.MessageReceivedEvent @event,
        CancellationToken cancellationToken = default)
    {
        var dto = new Contracts.MessageReceivedEvent(
            @event.ProjectId,
            @event.Message
        );

        await _hubContext.Clients
            .Group($"project-{@event.ProjectId}")
            .MessageReceived(dto);
    }
}
