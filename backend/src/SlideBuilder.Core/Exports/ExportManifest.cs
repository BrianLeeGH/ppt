namespace SlideBuilder.Core.Exports;

public class ExportManifest
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public List<string> Files { get; set; } = new List<string>();
}
