using SlideBuilder.Core.Artifacts;

namespace SlideBuilder.Core.Compilation;

public interface IPugCompilationService
{
    Task<string> CompileAsync(PresentationArtifact artifact, CancellationToken ct);
}
