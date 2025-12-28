using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.Artifacts;
using SlideBuilder.Core.Compilation;
using SlideBuilder.Core.Storage;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Jobs.Stages;

public class CompilePreviewStage : IJobStage
{
    public string Name => JobStages.CompilePreview;

    private readonly IUnitOfWork _uow;
    private readonly IPresentationArtifactService _artifactService;
    private readonly IPugCompilationService _compilationService;
    private readonly IObjectStorage _storage;
    private readonly ILogger<CompilePreviewStage> _logger;

    public CompilePreviewStage(
        IUnitOfWork uow,
        IPresentationArtifactService artifactService,
        IPugCompilationService compilationService,
        IObjectStorage storage,
        ILogger<CompilePreviewStage> logger)
    {
        _uow = uow;
        _artifactService = artifactService;
        _compilationService = compilationService;
        _storage = storage;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken ct)
    {
        _logger.LogInformation("Starting CompilePreview stage for job {JobId}", job.Id);

        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.GetSingleAsync(p => p.Id == job.ProjectId, "Deck.Slides");
        if (project == null) throw new Exception("Project not found");

        var assetRepo = _uow.GetRepository<Asset>();
        var assets = await assetRepo.FindAsync(a => a.ProjectId == project.Id);

        var artifact = _artifactService.Generate(project);

        foreach (var asset in assets)
        {
            artifact.AssetMappings[asset.Id.ToString()] = asset.Source;
        }

        var html = await _compilationService.CompileAsync(artifact, ct);

        // Save preview to storage
        var storageKey = $"previews/{project.Id}/index.html";
        using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html));
        await _storage.UploadAsync(storageKey, ms, "text/html", ct);

        _logger.LogInformation("Completed CompilePreview stage for job {JobId}. Preview saved to {StorageKey}", job.Id, storageKey);
    }
}
