using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.AI;
using SlideBuilder.Core.AI.Prompts;
using SlideBuilder.Core.AI.Parsing;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Jobs.Stages;

public class GenerateSlidesStage : IJobStage
{
    public string Name => JobStages.GenerateSlides;

    private readonly IUnitOfWork _uow;
    private readonly IModelClient _modelClient;
    private readonly ISlideGenerationPromptBuilder _promptBuilder;
    private readonly IModelOutputParser _parser;
    private readonly ILogger<GenerateSlidesStage> _logger;

    public GenerateSlidesStage(
        IUnitOfWork uow,
        IModelClient modelClient,
        ISlideGenerationPromptBuilder promptBuilder,
        IModelOutputParser parser,
        ILogger<GenerateSlidesStage> logger)
    {
        _uow = uow;
        _modelClient = modelClient;
        _promptBuilder = promptBuilder;
        _parser = parser;
        _logger = logger;
    }

    public async Task ExecuteAsync(Job job, CancellationToken ct)
    {
        _logger.LogInformation("Starting GenerateSlides stage for job {JobId}", job.Id);

        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.GetSingleAsync(p => p.Id == job.ProjectId, "Deck.DraftOutline", "Deck.DraftStyleBrief");
        if (project == null || project.Deck == null)
            throw new Exception("Project or Deck not found");

        var outline = project.Deck.DraftOutline;
        var styleBrief = project.Deck.DraftStyleBrief;

        if (outline == null || outline.Status != OutlineStatus.Approved)
            throw new Exception("Approved outline required");

        if (styleBrief == null || styleBrief.Status != StyleBriefStatus.Approved)
            throw new Exception("Approved style brief required");

        var prompt = _promptBuilder.Build(project);

        _logger.LogInformation("Calling AI to generate slides for project {ProjectId}", job.ProjectId);

        var response = await _modelClient.GenerateAsync(prompt, ct);

        var slides = _parser.ParseSlides(response, project.Deck.Id);

        // Clear existing slides and add new ones
        var slideRepo = _uow.GetRepository<Slide>();
        var existingSlides = await slideRepo.FindAsync(s => s.DeckId == project.Deck.Id);
        foreach (var s in existingSlides)
        {
            await slideRepo.DeleteAsync(s);
        }

        foreach (var s in slides)
        {
            await slideRepo.AddAsync(s);
        }

        await _uow.SaveChangesAsync();

        _logger.LogInformation("Completed GenerateSlides stage for job {JobId}. Generated {Count} slides.", job.Id, slides.Count);
    }
}
