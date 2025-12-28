using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Jobs.Stages;

public class DraftOutlineStage : IJobStage
{
    public string Name => JobStages.DraftOutline;

    private readonly IUnitOfWork _uow;
    private readonly ILogger<DraftOutlineStage> _logger;

    public DraftOutlineStage(IUnitOfWork uow, ILogger<DraftOutlineStage> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken ct)
    {
        _logger.LogInformation("Starting DraftOutline stage for job {JobId}", job.Id);

        // This stage might be used for a job that generates the initial outline from a prompt
        // For the "Generate Slides" job, this stage might be skipped if outline is already approved

        await Task.Delay(500, ct);

        _logger.LogInformation("Completed DraftOutline stage for job {JobId}", job.Id);
    }
}
