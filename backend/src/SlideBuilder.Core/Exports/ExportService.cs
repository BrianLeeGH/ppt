using System.IO.Compression;
using System.Text.Json;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.Artifacts;
using SlideBuilder.Core.Compilation;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Exports;

public interface IExportService
{
    Task<Stream> CreateExportAsync(Guid projectId, CancellationToken ct);
}

public class ExportService : IExportService
{
    private readonly IUnitOfWork _uow;
    private readonly IPresentationArtifactService _artifactService;
    private readonly IPugCompilationService _compilationService;
    private readonly ILogger<ExportService> _logger;

    public ExportService(
        IUnitOfWork uow,
        IPresentationArtifactService artifactService,
        IPugCompilationService compilationService,
        ILogger<ExportService> logger)
    {
        _uow = uow;
        _artifactService = artifactService;
        _compilationService = compilationService;
        _logger = logger;
    }

    public async Task<Stream> CreateExportAsync(Guid projectId, CancellationToken ct)
    {
        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.GetByIdAsync(projectId);
        if (project == null) throw new Exception("Project not found");

        var artifact = _artifactService.Generate(project);
        var html = await _compilationService.CompileAsync(artifact, ct);

        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // index.html
            var htmlEntry = archive.CreateEntry("index.html");
            using (var writer = new StreamWriter(htmlEntry.Open()))
            {
                await writer.WriteAsync(html);
            }

            // style.css
            var cssEntry = archive.CreateEntry("style.css");
            using (var writer = new StreamWriter(cssEntry.Open()))
            {
                await writer.WriteAsync(artifact.CssContent);
            }

            // script.js
            var jsEntry = archive.CreateEntry("script.js");
            using (var writer = new StreamWriter(jsEntry.Open()))
            {
                await writer.WriteAsync(artifact.JsContent);
            }

            // manifest.json
            var manifest = new ExportManifest
            {
                ProjectId = project.Id,
                Title = project.Deck?.Title ?? "Presentation",
                ExportedAt = DateTime.UtcNow,
                Files = new List<string> { "index.html", "style.css", "script.js" }
            };
            var manifestEntry = archive.CreateEntry("manifest.json");
            using (var writer = new StreamWriter(manifestEntry.Open()))
            {
                await writer.WriteAsync(JsonSerializer.Serialize(manifest));
            }
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}
