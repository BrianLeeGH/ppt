using System.Text;
using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.AI.Prompts;

public interface ISlideGenerationPromptBuilder
{
    string Build(Project project);
}

public class SlideGenerationPromptBuilder : ISlideGenerationPromptBuilder
{
    public string Build(Project project)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert presentation designer. Your task is to generate detailed slide content based on an approved outline and style brief.");
        sb.AppendLine();

        sb.AppendLine("### Project Context");
        sb.AppendLine($"Title: {project.Deck?.Title ?? "Untitled"}");
        sb.AppendLine($"Intent: {project.Deck?.IntentSummary ?? "Not specified"}");
        sb.AppendLine();

        sb.AppendLine("### Approved Outline");
        sb.AppendLine(project.Deck?.DraftOutline?.SlidesJson ?? "[]");
        sb.AppendLine();

        sb.AppendLine("### Style Brief");
        sb.AppendLine(project.Deck?.DraftStyleBrief?.FieldsJson ?? "{}");
        sb.AppendLine();

        sb.AppendLine("### Instructions");
        sb.AppendLine("1. Generate a JSON array of slides.");
        sb.AppendLine("2. Each slide must have: 'title', 'contentBlocks' (array of { type: 'text'|'image', value: string }), and 'speakerNotes'.");
        sb.AppendLine("3. Follow the style brief for tone and density.");
        sb.AppendLine("4. Ensure the content is engaging and professional.");
        sb.AppendLine("5. Return ONLY the JSON array.");

        return sb.ToString();
    }
}
