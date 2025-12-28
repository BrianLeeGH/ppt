using Aliyun.OSS;
using Microsoft.Extensions.Options;
using SlideBuilder.Core.Configuration;
using SlideBuilder.Core.Storage;

namespace SlideBuilder.Infrastructure.Storage;

public class OssObjectStorage : IObjectStorage
{
    private readonly OssSettings _settings;
    private readonly IOss _client;

    public OssObjectStorage(IOptions<AppSettings> settings)
    {
        _settings = settings.Value.Oss;
        _client = new OssClient(_settings.Endpoint, _settings.AccessKeyId, _settings.AccessKeySecret);
    }

    public async Task<string> UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var fullKey = string.IsNullOrEmpty(_settings.Prefix) ? key : $"{_settings.Prefix}/{key}";
        var metadata = new ObjectMetadata { ContentType = contentType };
        _client.PutObject(_settings.Bucket, fullKey, content, metadata);
        return await GetUrlAsync(key, ct);
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken ct = default)
    {
        var fullKey = string.IsNullOrEmpty(_settings.Prefix) ? key : $"{_settings.Prefix}/{key}";
        var ossObject = _client.GetObject(_settings.Bucket, fullKey);
        return ossObject.Content;
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        var fullKey = string.IsNullOrEmpty(_settings.Prefix) ? key : $"{_settings.Prefix}/{key}";
        _client.DeleteObject(_settings.Bucket, fullKey);
        await Task.CompletedTask;
    }

    public async Task<string> GetUrlAsync(string key, CancellationToken ct = default)
    {
        var fullKey = string.IsNullOrEmpty(_settings.Prefix) ? key : $"{_settings.Prefix}/{key}";
        // For MVP, we'll generate a signed URL or a public URL depending on bucket config.
        // Here we'll just return a simple URL assuming public read or handled by proxy.
        var url = $"{_settings.Endpoint.Replace("http://", $"http://{_settings.Bucket}.").Replace("https://", $"https://{_settings.Bucket}.")}/{fullKey}";
        return await Task.FromResult(url);
    }
}
