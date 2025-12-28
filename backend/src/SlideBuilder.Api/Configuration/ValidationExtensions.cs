using SlideBuilder.Core.Configuration;

namespace SlideBuilder.Api.Configuration;

public static class ValidationExtensions
{
    public static void ValidateSettings(this IServiceProvider services)
    {
        var settings = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AppSettings>>().Value;

        if (string.IsNullOrEmpty(settings.Ai.ApiKey) || settings.Ai.ApiKey == "YOUR_API_KEY")
        {
            throw new InvalidOperationException("AI API Key is not configured in appsettings.json or environment variables.");
        }

        if (string.IsNullOrEmpty(settings.Oss.AccessKeyId) || settings.Oss.AccessKeyId == "YOUR_ACCESS_KEY_ID")
        {
            throw new InvalidOperationException("OSS Access Key ID is not configured.");
        }

        if (string.IsNullOrEmpty(settings.Oss.Bucket) || settings.Oss.Bucket == "YOUR_BUCKET_NAME")
        {
            throw new InvalidOperationException("OSS Bucket Name is not configured.");
        }
    }
}
