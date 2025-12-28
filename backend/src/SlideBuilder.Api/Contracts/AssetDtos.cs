namespace SlideBuilder.Api.Contracts;

public record AssetDto(
    Guid Id,
    Guid ProjectId,
    string Kind,
    string OriginalName,
    string ContentType,
    long SizeBytes,
    string StorageKey,
    string Source,
    DateTime CreatedAt
);

public record ImportAssetRequest(
    string Url,
    string? OriginalName
);
