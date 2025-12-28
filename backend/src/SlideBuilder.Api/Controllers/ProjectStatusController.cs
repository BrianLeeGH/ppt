using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{id}/status")]
public class ProjectStatusController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ProjectStatusController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    public async Task<ActionResult<ProjectStatusDto>> GetProjectStatus(Guid id)
    {
        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.GetByIdAsync(id);
        if (project == null) return NotFound();

        var jobRepo = _uow.GetRepository<Job>();
        var latestJob = (await jobRepo.FindAsync(j => j.ProjectId == id))
            .OrderByDescending(j => j.CreatedAt)
            .FirstOrDefault();

        return Ok(new ProjectStatusDto(
            project.Id,
            project.Name,
            project.Deck?.DraftOutline?.Status.ToString() ?? "None",
            project.Deck?.DraftStyleBrief?.Status.ToString() ?? "None",
            project.Deck?.Slides.Count ?? 0,
            latestJob?.Status.ToString() ?? "None",
            latestJob?.Stage ?? "None"
        ));
    }
}

public record ProjectStatusDto(
    Guid Id,
    string Name,
    string OutlineStatus,
    string StyleBriefStatus,
    int SlideCount,
    string LatestJobStatus,
    string LatestJobStage
);
