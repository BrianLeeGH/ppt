using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using System.Text.Json;

namespace SlideBuilder.Core.Services.Outlines;

public interface IOutlineService
{
    Task<Outline?> GetOutlineByProjectIdAsync(Guid projectId);
    Task<Outline?> UpdateOutlineAsync(Guid projectId, string slidesJson);
    Task<Outline?> ApproveOutlineAsync(Guid projectId);
}

public class OutlineService : IOutlineService
{
    private readonly IUnitOfWork _uow;

    public OutlineService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Outline?> GetOutlineByProjectIdAsync(Guid projectId)
    {
        var project = await _uow.GetRepository<Project>().GetByIdAsync(projectId);
        return project?.Deck?.DraftOutline;
    }

    public async Task<Outline?> UpdateOutlineAsync(Guid projectId, string slidesJson)
    {
        var project = await _uow.GetRepository<Project>().GetByIdAsync(projectId);
        if (project?.Deck?.DraftOutline == null) return null;

        project.Deck.DraftOutline.SlidesJson = slidesJson;
        project.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return project.Deck.DraftOutline;
    }

    public async Task<Outline?> ApproveOutlineAsync(Guid projectId)
    {
        var project = await _uow.GetRepository<Project>().GetByIdAsync(projectId);
        if (project?.Deck?.DraftOutline == null) return null;

        project.Deck.DraftOutline.Status = OutlineStatus.Approved;
        project.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return project.Deck.DraftOutline;
    }
}
