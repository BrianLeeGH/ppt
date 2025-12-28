using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.AI;
using SlideBuilder.Core.AI.Prompts;
using SlideBuilder.Core.AI.Parsing;
using Microsoft.Extensions.Logging;

namespace SlideBuilder.Core.Services.Slides;

public interface ISlideEditService
{
    Task<Slide> RegenerateSlideAsync(Guid slideId, string? instruction, CancellationToken ct);
}

public class SlideEditService : ISlideEditService
{
    private readonly IUnitOfWork _uow;
    private readonly IModelClient _modelClient;
    private readonly IModelOutputParser _parser;
    private readonly ILogger<SlideEditService> _logger;

    public SlideEditService(IUnitOfWork uow, IModelClient modelClient, IModelOutputParser parser, ILogger<SlideEditService> logger)
    {
        _uow = uow;
        _modelClient = modelClient;
        _parser = parser;
        _logger = logger;
    }

    public async Task<Slide> RegenerateSlideAsync(Guid slideId, string? instruction, CancellationToken ct)
    {
        var slideRepo = _uow.GetRepository<Slide>();
        var slide = await slideRepo.GetByIdAsync(slideId);
        if (slide == null) throw new Exception("Slide not found");

        var projectRepo = _uow.GetRepository<Project>();
        var project = await projectRepo.FindAsync(p => p.Deck != null && p.Deck.Id == slide.DeckId, "Deck.DraftOutline", "Deck.DraftStyleBrief");
        var proj = project.FirstOrDefault();
        if (proj == null) throw new Exception("Project not found");

        var prompt = BuildRegenerationPrompt(proj, slide, instruction);

        var response = await _modelClient.GenerateAsync(prompt, ct);
        var newSlides = _parser.ParseSlides(response, slide.DeckId);

        if (newSlides.Count == 0) throw new Exception("AI failed to generate slide content");

        var newSlide = newSlides[0];
        slide.Title = newSlide.Title;
        slide.ContentBlocksJson = newSlide.ContentBlocksJson;
        slide.SpeakerNotes = newSlide.SpeakerNotes;

        await slideRepo.UpdateAsync(slide);
        await _uow.SaveChangesAsync();

        return slide;
    }

    private string BuildRegenerationPrompt(Project project, Slide slide, string? instruction)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("You are an expert presentation designer.");
        sb.AppendLine("Your task is to REGENERATE a specific slide based on the project context and a specific instruction.");
        sb.AppendLine();
        sb.AppendLine("### Project Context");
        sb.AppendLine($"Title: {project.Deck?.Title}");
        sb.AppendLine($"Style: {project.Deck?.DraftStyleBrief?.FieldsJson}");
        sb.AppendLine();
        sb.AppendLine("### Current Slide Content");
        sb.AppendLine($"Title: {slide.Title}");
        sb.AppendLine($"Content: {slide.ContentBlocksJson}");
        sb.AppendLine();
        sb.AppendLine("### Instruction for Change");
        sb.AppendLine(instruction ?? "Improve the content and make it more engaging.");
        sb.AppendLine();
        sb.AppendLine("### Output Format");
        sb.AppendLine("Return a JSON array containing exactly ONE slide object with 'title', 'contentBlocks', and 'speakerNotes'.");

        return sb.ToString();
    }
}
