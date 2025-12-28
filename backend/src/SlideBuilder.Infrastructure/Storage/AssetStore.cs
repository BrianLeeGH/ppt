using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.Storage;

namespace SlideBuilder.Infrastructure.Storage;

public interface IAssetStore
{
    Task<Asset> SaveAssetAsync(Guid projectId, Stream stream, string fileName, string contentType, string source, CancellationToken ct);
    Task<Asset> ImportFromUrlAsync(Guid projectId, string url, string? fileName, CancellationToken ct);
}

public class AssetStore : IAssetStore
{
    private readonly IUnitOfWork _uow;
    private readonly IObjectStorage _storage;
    private readonly HttpClient _httpClient;

    public AssetStore(IUnitOfWork uow, IObjectStorage storage, HttpClient httpClient)
    {
        _uow = uow;
        _storage = storage;
        _httpClient = httpClient;
    }

    public async Task<Asset> SaveAssetAsync(Guid projectId, Stream stream, string fileName, string contentType, string source, CancellationToken ct)
    {
        var assetId = Guid.NewGuid();
        var extension = Path.GetExtension(fileName);
        var storageKey = $"assets/{projectId}/{assetId}{extension}";

        await _storage.UploadAsync(storageKey, stream, contentType, ct);

        var asset = new Asset
        {
            Id = assetId,
            ProjectId = projectId,
            Kind = contentType.StartsWith("image/") ? "Image" : "Other",
            OriginalName = fileName,
            ContentType = contentType,
            SizeBytes = stream.Length,
            StorageKey = storageKey,
            Source = source,
            CreatedAt = DateTime.UtcNow
        };

        var assetRepo = _uow.GetRepository<Asset>();
        await assetRepo.AddAsync(asset);
        await _uow.SaveChangesAsync();

        return asset;
    }

    public async Task<Asset> ImportFromUrlAsync(Guid projectId, string url, string? fileName, CancellationToken ct)
    {
        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
        var name = fileName ?? Path.GetFileName(new Uri(url).LocalPath) ?? "imported-asset";

        return await SaveAssetAsync(projectId, stream, name, contentType, "URL", ct);
    }
}
