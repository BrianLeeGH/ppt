using SlideBuilder.Core.Domain;

namespace SlideBuilder.Api.Contracts;

public record JobDto(
    Guid Id,
    Guid ProjectId,
    JobStatus Status,
    string Stage,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Error,
    string? InputSummary
);

public record CreateJobRequest(
    Guid ProjectId
);

public record JobStatusResponse(
    Guid Id,
    JobStatus Status,
    string Stage,
    string? Error
);
