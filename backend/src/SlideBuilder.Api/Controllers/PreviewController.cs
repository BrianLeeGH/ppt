using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Core.Storage;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/preview")]
public class PreviewController : ControllerBase
{
    private readonly IObjectStorage _storage;

    public PreviewController(IObjectStorage storage)
    {
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreview(Guid projectId)
    {
        var storageKey = $"previews/{projectId}/index.html";
        try
        {
            var stream = await _storage.DownloadAsync(storageKey, CancellationToken.None);
            return File(stream, "text/html");
        }
        catch (Exception)
        {
            return NotFound("Preview not found. Has the job completed?");
        }
    }
}
