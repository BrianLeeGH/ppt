using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SlideBuilder.Core.Artifacts;
using SlideBuilder.Core.Compilation;

namespace SlideBuilder.Infrastructure.Compilation;

public class PugCompilationService : IPugCompilationService
{
    private readonly ILogger<PugCompilationService> _logger;
    private readonly string _compilerPath;

    public PugCompilationService(ILogger<PugCompilationService> logger)
    {
        _logger = logger;
        // Path to tools/pug-compiler/index.js relative to the API project or absolute
        _compilerPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../tools/pug-compiler/index.js"));
    }

    public async Task<string> CompileAsync(PresentationArtifact artifact, CancellationToken ct)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SlideBuilder", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var pugFile = Path.Combine(tempDir, "input.pug");
        var htmlFile = Path.Combine(tempDir, "output.html");

        var pugContent = artifact.PugContent;
        foreach (var mapping in artifact.AssetMappings)
        {
            pugContent = pugContent.Replace($"asset://{mapping.Key}", mapping.Value);
        }

        await File.WriteAllTextAsync(pugFile, pugContent, ct);

        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = $"\"{_compilerPath}\" \"{pugFile}\" \"{htmlFile}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(ct);
        var error = await process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        if (process.ExitCode != 0)
        {
            _logger.LogError("Pug compilation failed: {Error}", error);
            throw new Exception($"Pug compilation failed: {error}");
        }

        var html = await File.ReadAllTextAsync(htmlFile, ct);

        // Cleanup
        try { Directory.Delete(tempDir, true); } catch { /* ignore */ }

        return html;
    }
}
