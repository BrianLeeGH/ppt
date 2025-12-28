using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Core.Services.Projects;

public interface IProjectService
{
    Task<Project> CreateProjectAsync(string name);
    Task<IEnumerable<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectByIdAsync(Guid id);
}

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _uow;

    public ProjectService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Project> CreateProjectAsync(string name)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Deck = new Deck
            {
                Id = Guid.NewGuid(),
                DraftOutline = new Outline
                {
                    Id = Guid.NewGuid(),
                    Status = OutlineStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                },
                DraftStyleBrief = new StyleBrief
                {
                    Id = Guid.NewGuid(),
                    Status = StyleBriefStatus.Draft,
                    CreatedAt = DateTime.UtcNow
                }
            }
        };

        await _uow.GetRepository<Project>().AddAsync(project);
        await _uow.SaveChangesAsync();

        return project;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync()
    {
        return await _uow.GetRepository<Project>().GetAllAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        return await _uow.GetRepository<Project>().GetByIdAsync(id);
    }
}
