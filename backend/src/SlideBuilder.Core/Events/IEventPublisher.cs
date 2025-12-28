namespace SlideBuilder.Core.Events;

public interface IEventPublisher
{
    Task PublishOutlineUpdatedAsync(OutlineUpdatedEvent @event, CancellationToken cancellationToken = default);
    Task PublishMessageReceivedAsync(MessageReceivedEvent @event, CancellationToken cancellationToken = default);
}

public record OutlineUpdatedEvent(
    Guid ProjectId,
    Guid OutlineId,
    object Outline,
    int RevisionNumber,
    List<int> ChangedSlideIndices
);

public record MessageReceivedEvent(
    Guid ProjectId,
    object Message
);
