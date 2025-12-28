using System.Text;
using System.Text.Json;
using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.Artifacts;

public interface IPresentationArtifactService
{
    PresentationArtifact Generate(Project project);
}

public class PresentationArtifactService : IPresentationArtifactService
{
    public PresentationArtifact Generate(Project project)
    {
        if (project.Deck == null) throw new Exception("Deck not found");

        var pug = new StringBuilder();
        pug.AppendLine("doctype html");
        pug.AppendLine("html");
        pug.AppendLine("  head");
        pug.AppendLine($"    title {project.Deck.Title ?? "Presentation"}");
        pug.AppendLine("    link(rel='stylesheet', href='style.css')");
        pug.AppendLine("  body");
        pug.AppendLine("    .reveal");
        pug.AppendLine("      .slides");

        foreach (var slide in project.Deck.Slides.OrderBy(s => s.OrderIndex))
        {
            pug.AppendLine("        section");
            pug.AppendLine($"          h2 {slide.Title}");

            var blocks = JsonSerializer.Deserialize<List<ContentBlock>>(slide.ContentBlocksJson) ?? new List<ContentBlock>();
            foreach (var block in blocks)
            {
                if (block.Type == "text")
                {
                    pug.AppendLine($"          p {block.Value}");
                }
                else if (block.Type == "image")
                {
                    pug.AppendLine($"          img(src='{block.Value}')");
                }
            }

            if (!string.IsNullOrEmpty(slide.SpeakerNotes))
            {
                pug.AppendLine("          aside.notes");
                pug.AppendLine($"            | {slide.SpeakerNotes}");
            }
        }

        pug.AppendLine("    script(src='script.js')");

        return new PresentationArtifact
        {
            PugContent = pug.ToString(),
            CssContent = "/* Default styles */ body { font-family: sans-serif; }",
            JsContent = "// Default scripts"
        };
    }

    private class ContentBlock
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
    }
}
