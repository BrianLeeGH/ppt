using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Core.Exports;
using SlideBuilder.Infrastructure.Exports;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/export")]
public class ExportController : ControllerBase
{
    private readonly IExportService _exportService;
    private readonly IExportStorageWriter _storageWriter;

    public ExportController(IExportService exportService, IExportStorageWriter storageWriter)
    {
        _exportService = exportService;
        _storageWriter = storageWriter;
    }

    [HttpPost]
    public async Task<IActionResult> CreateExport(Guid projectId)
    {
        try
        {
            var zipStream = await _exportService.CreateExportAsync(projectId, CancellationToken.None);
            var storageKey = await _storageWriter.SaveExportAsync(projectId, zipStream, CancellationToken.None);

            return Ok(new { storageKey });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("download")]
    public async Task<IActionResult> DownloadExport(Guid projectId)
    {
        try
        {
            var zipStream = await _exportService.CreateExportAsync(projectId, CancellationToken.None);
            return File(zipStream, "application/zip", $"presentation-{projectId}.zip");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
