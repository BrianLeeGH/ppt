using SlideBuilder.Core.Storage;

namespace SlideBuilder.Infrastructure.Exports;

public interface IExportStorageWriter
{
    Task<string> SaveExportAsync(Guid projectId, Stream zipStream, CancellationToken ct);
}

public class ExportStorageWriter : IExportStorageWriter
{
    private readonly IObjectStorage _storage;

    public ExportStorageWriter(IObjectStorage storage)
    {
        _storage = storage;
    }

    public async Task<string> SaveExportAsync(Guid projectId, Stream zipStream, CancellationToken ct)
    {
        var storageKey = $"exports/{projectId}/{Guid.NewGuid()}.zip";
        await _storage.UploadAsync(storageKey, zipStream, "application/zip", ct);
        return storageKey;
    }
}
