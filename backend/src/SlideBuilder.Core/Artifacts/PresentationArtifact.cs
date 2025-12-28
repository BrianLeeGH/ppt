namespace SlideBuilder.Core.Artifacts;

public class PresentationArtifact
{
    public string PugContent { get; set; } = string.Empty;
    public string CssContent { get; set; } = string.Empty;
    public string JsContent { get; set; } = string.Empty;
    public Dictionary<string, string> AssetMappings { get; set; } = new();
}
