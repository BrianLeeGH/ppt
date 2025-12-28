namespace SlideBuilder.Core.Configuration;

public class AppSettings
{
    public AiSettings Ai { get; set; } = new();
    public OssSettings Oss { get; set; } = new();
}

public class AiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public int ContextWindowSize { get; set; } = 20;
    public int SummaryThreshold { get; set; } = 30;
}

public class OssSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public string Prefix { get; set; } = string.Empty;
}
