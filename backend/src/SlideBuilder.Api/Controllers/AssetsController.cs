using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;
using SlideBuilder.Core.Services.Slides;
using SlideBuilder.Infrastructure.Storage;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/assets")]
public class AssetsController : ControllerBase
{
    private readonly IAssetStore _assetStore;
    private readonly IUnitOfWork _uow;

    public AssetsController(IAssetStore assetStore, IUnitOfWork uow)
    {
        _assetStore = assetStore;
        _uow = uow;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AssetDto>>> GetAssets(Guid projectId)
    {
        var assetRepo = _uow.GetRepository<Asset>();
        var assets = await assetRepo.FindAsync(a => a.ProjectId == projectId);
        return Ok(assets.Select(MapToDto));
    }

    [HttpPost("upload")]
    public async Task<ActionResult<AssetDto>> UploadAsset(Guid projectId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");

        using var stream = file.OpenReadStream();
        var asset = await _assetStore.SaveAssetAsync(
            projectId,
            stream,
            file.FileName,
            file.ContentType,
            "Upload",
            CancellationToken.None);

        return Ok(MapToDto(asset));
    }

    [HttpPost("import")]
    public async Task<ActionResult<AssetDto>> ImportAsset(Guid projectId, [FromBody] ImportAssetRequest request)
    {
        try
        {
            var asset = await _assetStore.ImportFromUrlAsync(
                projectId,
                request.Url,
                request.OriginalName,
                CancellationToken.None);
            return Ok(MapToDto(asset));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{assetId}/insert/{slideId}")]
    public async Task<IActionResult> InsertIntoSlide(Guid projectId, Guid assetId, Guid slideId, [FromServices] ISlideAssetInsertService insertService)
    {
        await insertService.InsertAssetAsync(slideId, assetId, CancellationToken.None);
        return Ok();
    }

    private static AssetDto MapToDto(Asset asset) => new(
        asset.Id,
        asset.ProjectId,
        asset.Kind,
        asset.OriginalName,
        asset.ContentType,
        asset.SizeBytes,
        asset.StorageKey,
        asset.Source,
        asset.CreatedAt
    );
}
