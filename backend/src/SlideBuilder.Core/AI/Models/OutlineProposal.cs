namespace SlideBuilder.Core.AI.Models;

public class OutlineProposal
{
    public List<SlideProposal> Slides { get; set; } = new();
    public bool NeedsClarification { get; set; }
    public List<string>? ClarificationQuestions { get; set; }
}

public class SlideProposal
{
    public string Title { get; set; } = string.Empty;
    public List<string> KeyPoints { get; set; } = new();
}
