using System.Text.Json;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Persistence;

namespace SlideBuilder.Core.Services.Slides;

public interface ISlideAssetInsertService
{
    Task InsertAssetAsync(Guid slideId, Guid assetId, CancellationToken ct);
}

public class SlideAssetInsertService : ISlideAssetInsertService
{
    private readonly IUnitOfWork _unitOfWork;

    public SlideAssetInsertService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task InsertAssetAsync(Guid slideId, Guid assetId, CancellationToken ct)
    {
        var slideRepo = _unitOfWork.GetRepository<Slide>();
        var slide = await slideRepo.GetByIdAsync(slideId);
        if (slide == null) throw new Exception("Slide not found");

        var assetRepo = _unitOfWork.GetRepository<Asset>();
        var asset = await assetRepo.GetByIdAsync(assetId);
        if (asset == null) throw new Exception("Asset not found");

        var blocks = JsonSerializer.Deserialize<List<ContentBlock>>(slide.ContentBlocksJson) ?? new List<ContentBlock>();

        blocks.Add(new ContentBlock
        {
            Type = "Image",
            AssetId = asset.Id,
            AssetUrl = asset.Source // For MVP, we use the source URL or storage key
        });

        slide.ContentBlocksJson = JsonSerializer.Serialize(blocks);
        await slideRepo.UpdateAsync(slide);
        await _unitOfWork.SaveChangesAsync();
    }
}
