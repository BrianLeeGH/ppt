using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Jobs;

public interface IJobStage
{
    string Name { get; }
    Task ExecuteAsync(Job job, CancellationToken ct);
}

public interface IJobRunner
{
    Task RunAsync(Guid jobId, CancellationToken ct);
}

public class JobRunner : IJobRunner
{
    private readonly IEnumerable<IJobStage> _stages;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<JobRunner> _logger;

    public JobRunner(IEnumerable<IJobStage> stages, IUnitOfWork uow, ILogger<JobRunner> logger)
    {
        _stages = stages;
        _uow = uow;
        _logger = logger;
    }

    public async Task RunAsync(Guid jobId, CancellationToken ct)
    {
        var jobRepo = _uow.GetRepository<Job>();
        var job = await jobRepo.GetByIdAsync(jobId);
        if (job == null) return;

        job.Status = JobStatus.Running;
        job.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        try
        {
            foreach (var stage in _stages)
            {
                if (ct.IsCancellationRequested)
                {
                    job.Status = JobStatus.Stopped;
                    break;
                }

                // Skip stages already completed (for resume)
                if (job.Checkpoints.Any(c => c.StageCompleted == stage.Name))
                {
                    continue;
                }

                job.Stage = stage.Name;
                await _uow.SaveChangesAsync();

                await stage.ExecuteAsync(job, ct);

                var checkpoint = new Checkpoint
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    StageCompleted = stage.Name,
                    CreatedAt = DateTime.UtcNow
                };
                job.Checkpoints.Add(checkpoint);
                await _uow.SaveChangesAsync();
            }

            if (job.Status != JobStatus.Stopped)
            {
                job.Status = JobStatus.Succeeded;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed at stage {Stage}", job.Id, job.Stage);
            job.Status = JobStatus.Failed;
            job.Error = ex.Message;
        }
        finally
        {
            job.UpdatedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();
        }
    }
}
