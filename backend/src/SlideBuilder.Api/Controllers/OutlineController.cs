using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Services.Outlines;
using System.Text.Json;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
public class OutlineController : ControllerBase
{
    private readonly IOutlineService _outlineService;

    public OutlineController(IOutlineService outlineService)
    {
        _outlineService = outlineService;
    }

    [HttpGet]
    public async Task<ActionResult<OutlineDto>> GetOutline(Guid projectId)
    {
        var outline = await _outlineService.GetOutlineByProjectIdAsync(projectId);
        if (outline == null) return NotFound();

        return Ok(MapToDto(outline));
    }

    [HttpPut]
    public async Task<ActionResult<OutlineDto>> UpdateOutline(Guid projectId, UpdateOutlineRequest request)
    {
        var slidesJson = JsonSerializer.Serialize(request.Slides);
        var outline = await _outlineService.UpdateOutlineAsync(projectId, slidesJson);
        if (outline == null) return NotFound();

        return Ok(MapToDto(outline));
    }

    [HttpPost("approve")]
    public async Task<ActionResult<OutlineDto>> ApproveOutline(Guid projectId)
    {
        var outline = await _outlineService.ApproveOutlineAsync(projectId);
        if (outline == null) return NotFound();

        return Ok(MapToDto(outline));
    }

    private static OutlineDto MapToDto(Outline outline)
    {
        var slides = JsonSerializer.Deserialize<List<OutlineSlideDto>>(outline.SlidesJson) ?? new List<OutlineSlideDto>();
        return new OutlineDto(outline.Id, outline.DeckId, outline.Status.ToString(), slides, outline.CreatedAt);
    }
}
