namespace SlideBuilder.Api.Contracts;

public record ProjectDto(Guid Id, string Name, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateProjectRequest(string Name);

public record OutlineDto(Guid Id, Guid DeckId, string Status, List<OutlineSlideDto> Slides, DateTime CreatedAt);

public record OutlineSlideDto(string Title, List<string> KeyPoints);

public record UpdateOutlineRequest(List<OutlineSlideDto> Slides);
