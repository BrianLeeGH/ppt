namespace SlideBuilder.Core.Storage;

public interface IObjectStorage
{
    Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
    Task<string> GetUrlAsync(string key, CancellationToken ct = default);
}
