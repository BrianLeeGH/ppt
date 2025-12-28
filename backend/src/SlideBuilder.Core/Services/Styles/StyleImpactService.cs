using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using System.Text.Json;

namespace SlideBuilder.Core.Services.Styles;

public interface IStyleImpactService
{
    Task<string> GenerateImpactSummaryAsync(Guid projectId, string newFieldsJson);
}

public class StyleImpactService : IStyleImpactService
{
    private readonly IUnitOfWork _uow;

    public StyleImpactService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<string> GenerateImpactSummaryAsync(Guid projectId, string newFieldsJson)
    {
        var project = await _uow.GetRepository<Project>().GetByIdAsync(projectId);
        var oldFieldsJson = project?.Deck?.DraftStyleBrief?.FieldsJson;

        if (string.IsNullOrEmpty(oldFieldsJson) || oldFieldsJson == "{}")
        {
            return "Initial style definition.";
        }

        // Simple diff for MVP. In a real app, we might use AI to summarize the impact.
        return "Style changes detected. This may affect the visual appearance of all slides.";
    }
}
