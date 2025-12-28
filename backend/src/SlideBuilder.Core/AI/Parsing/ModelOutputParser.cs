using System.Text.Json;
using SlideBuilder.Core.Domain;

namespace SlideBuilder.Core.AI.Parsing;

public interface IModelOutputParser
{
    List<Slide> ParseSlides(string output, Guid deckId);
}

public class ModelOutputParser : IModelOutputParser
{
    public List<Slide> ParseSlides(string output, Guid deckId)
    {
        // Clean up output if it contains markdown code blocks
        var json = output.Trim();
        if (json.StartsWith("```json"))
        {
            json = json.Substring(7);
            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }
        }
        else if (json.StartsWith("```"))
        {
            json = json.Substring(3);
            if (json.EndsWith("```"))
            {
                json = json.Substring(0, json.Length - 3);
            }
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rawSlides = JsonSerializer.Deserialize<List<RawSlide>>(json, options);

            if (rawSlides == null) return new List<Slide>();

            return rawSlides.Select((s, index) => new Slide
            {
                Id = Guid.NewGuid(),
                DeckId = deckId,
                OrderIndex = index,
                Title = s.Title ?? string.Empty,
                ContentBlocksJson = JsonSerializer.Serialize(s.ContentBlocks ?? new List<RawContentBlock>()),
                SpeakerNotes = s.SpeakerNotes
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to parse AI output as slides: {ex.Message}. Output was: {output}", ex);
        }
    }

    private class RawSlide
    {
        public string? Title { get; set; }
        public List<RawContentBlock>? ContentBlocks { get; set; }
        public string? SpeakerNotes { get; set; }
    }

    private class RawContentBlock
    {
        public string? Type { get; set; }
        public string? Value { get; set; }
    }
}
