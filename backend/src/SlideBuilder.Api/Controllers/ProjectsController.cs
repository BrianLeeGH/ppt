using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Services.Projects;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects.Select(p => new ProjectDto(p.Id, p.Name, p.CreatedAt, p.UpdatedAt)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDto>> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null) return NotFound();
        return Ok(new ProjectDto(project.Id, project.Name, project.CreatedAt, project.UpdatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<ProjectDto>> CreateProject(CreateProjectRequest request)
    {
        var project = await _projectService.CreateProjectAsync(request.Name);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, new ProjectDto(project.Id, project.Name, project.CreatedAt, project.UpdatedAt));
    }
}
