using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Api.Contracts;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Services.Styles;
using System.Text.Json;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/[controller]")]
public class StyleBriefController : ControllerBase
{
    private readonly IStyleBriefService _styleBriefService;

    public StyleBriefController(IStyleBriefService styleBriefService)
    {
        _styleBriefService = styleBriefService;
    }

    [HttpGet]
    public async Task<ActionResult<StyleBriefDto>> GetStyleBrief(Guid projectId)
    {
        var styleBrief = await _styleBriefService.GetStyleBriefByProjectIdAsync(projectId);
        if (styleBrief == null) return NotFound();

        return Ok(MapToDto(styleBrief));
    }

    [HttpPut]
    public async Task<ActionResult<StyleBriefDto>> UpdateStyleBrief(Guid projectId, UpdateStyleBriefRequest request)
    {
        var fieldsJson = JsonSerializer.Serialize(request.Fields);
        var styleBrief = await _styleBriefService.UpdateStyleBriefAsync(projectId, fieldsJson);
        if (styleBrief == null) return NotFound();

        return Ok(MapToDto(styleBrief));
    }

    [HttpPost("approve")]
    public async Task<ActionResult<StyleBriefDto>> ApproveStyleBrief(Guid projectId)
    {
        var styleBrief = await _styleBriefService.ApproveStyleBriefAsync(projectId);
        if (styleBrief == null) return NotFound();

        return Ok(MapToDto(styleBrief));
    }

    private static StyleBriefDto MapToDto(StyleBrief styleBrief)
    {
        var fields = JsonSerializer.Deserialize<StyleBriefFieldsDto>(styleBrief.FieldsJson) ?? new StyleBriefFieldsDto("", "", "", "", "", "", "", "", "", "");
        return new StyleBriefDto(styleBrief.Id, styleBrief.DeckId, styleBrief.Status.ToString(), fields, styleBrief.CreatedAt);
    }
}
