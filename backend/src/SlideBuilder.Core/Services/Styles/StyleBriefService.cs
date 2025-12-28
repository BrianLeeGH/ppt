using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Core.Services.Styles;

public interface IStyleBriefService
{
    Task<StyleBrief?> GetStyleBriefByProjectIdAsync(Guid projectId);
    Task<StyleBrief?> UpdateStyleBriefAsync(Guid projectId, string fieldsJson);
    Task<StyleBrief?> ApproveStyleBriefAsync(Guid projectId);
}

public class StyleBriefService : IStyleBriefService
{
    private readonly IUnitOfWork _uow;

    public StyleBriefService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<StyleBrief?> GetStyleBriefByProjectIdAsync(Guid projectId)
    {
        var project = await _uow.GetRepository<Project>().GetSingleAsync(p => p.Id == projectId, "Deck.DraftStyleBrief");
        return project?.Deck?.DraftStyleBrief;
    }

    public async Task<StyleBrief?> UpdateStyleBriefAsync(Guid projectId, string fieldsJson)
    {
        var project = await _uow.GetRepository<Project>().GetSingleAsync(p => p.Id == projectId, "Deck.DraftStyleBrief");
        if (project?.Deck?.DraftStyleBrief == null) return null;

        project.Deck.DraftStyleBrief.FieldsJson = fieldsJson;
        project.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return project.Deck.DraftStyleBrief;
    }

    public async Task<StyleBrief?> ApproveStyleBriefAsync(Guid projectId)
    {
        var project = await _uow.GetRepository<Project>().GetSingleAsync(p => p.Id == projectId, "Deck.DraftStyleBrief");
        if (project?.Deck?.DraftStyleBrief == null) return null;

        project.Deck.DraftStyleBrief.Status = StyleBriefStatus.Approved;
        project.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync();

        return project.Deck.DraftStyleBrief;
    }
}
