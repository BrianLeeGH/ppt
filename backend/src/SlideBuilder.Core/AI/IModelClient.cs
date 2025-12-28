namespace SlideBuilder.Core.AI;

public interface IModelClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken ct = default, string? systemMessage = null);
    Task<T?> GenerateStructuredAsync<T>(string prompt, CancellationToken ct = default, string? systemMessage = null) where T : class;
}
