using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Jobs;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IJobRunner _jobRunner;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IUnitOfWork uow, IJobRunner jobRunner, ILogger<JobsController> logger)
    {
        _uow = uow;
        _jobRunner = jobRunner;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<JobDto>> GetJob(Guid id)
    {
        var jobRepo = _uow.GetRepository<Job>();
        var job = await jobRepo.GetByIdAsync(id);
        if (job == null) return NotFound();

        return Ok(MapToDto(job));
    }

    [HttpGet("project/{projectId}")]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetProjectJobs(Guid projectId)
    {
        var jobRepo = _uow.GetRepository<Job>();
        var jobs = await jobRepo.FindAsync(j => j.ProjectId == projectId);
        return Ok(jobs.OrderByDescending(j => j.CreatedAt).Select(MapToDto));
    }

    [HttpPost]
    public async Task<ActionResult<JobDto>> CreateJob(CreateJobRequest request)
    {
        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.GetByIdAsync(request.ProjectId);
        if (project == null) return NotFound("Project not found");

        // Check if outline and style brief are approved
        if (project.Deck?.DraftOutline?.Status != OutlineStatus.Approved)
            return BadRequest("Outline must be approved before generating slides");

        if (project.Deck?.DraftStyleBrief?.Status != StyleBriefStatus.Approved)
            return BadRequest("Style brief must be approved before generating slides");

        var job = new Job
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Status = JobStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var jobRepo = _uow.GetRepository<Job>();
        await jobRepo.AddAsync(job);
        await _uow.SaveChangesAsync();

        // Fire and forget for MVP. In production use a real queue.
        _ = Task.Run(async () =>
        {
            try
            {
                await _jobRunner.RunAsync(job.Id, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running job {JobId}", job.Id);
            }
        });

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, MapToDto(job));
    }

    private static JobDto MapToDto(Job job) => new(
        job.Id,
        job.ProjectId,
        job.Status,
        job.Stage,
        job.CreatedAt,
        job.UpdatedAt,
        job.Error,
        job.InputSummary
    );
}
