using Microsoft.Extensions.Options;
using SlideBuilder.Core.Configuration;
using SlideBuilder.Core.AI;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SlideBuilder.Infrastructure.AI;

public class OpenAiCompatibleModelClient : IModelClient
{
    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;

    public OpenAiCompatibleModelClient(HttpClient httpClient, IOptions<AppSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value.Ai;
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default, string? systemMessage = null)
    {
        var requestBody = new
        {
            model = _settings.Model,
            messages = new List<object>
            {
                new { role = "system", content = systemMessage ?? "You are a helpful assistant." },
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl.TrimEnd('/')}/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseContent);
        return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
    }

    public async Task<T?> GenerateStructuredAsync<T>(string prompt, CancellationToken ct = default, string? systemMessage = null) where T : class
    {
        var content = await GenerateAsync(prompt, ct, systemMessage);

        // Try to extract JSON from markdown code blocks if present
        var json = content;
        if (content.Contains("```json"))
        {
            var start = content.IndexOf("```json") + 7;
            var end = content.LastIndexOf("```");
            json = content.Substring(start, end - start).Trim();
        }
        else if (content.Contains("```"))
        {
            var start = content.IndexOf("```") + 3;
            var end = content.LastIndexOf("```");
            json = content.Substring(start, end - start).Trim();
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}
