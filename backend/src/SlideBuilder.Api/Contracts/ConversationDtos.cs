namespace SlideBuilder.Api.Contracts;

public record SendMessageRequest(string Content);

public record ConversationMessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt
);

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
