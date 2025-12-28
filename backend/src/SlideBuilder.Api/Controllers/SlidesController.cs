using Microsoft.AspNetCore.Mvc;
using SlideBuilder.Core.Domain;
using SlideBuilder.Core.Services.Slides;

namespace SlideBuilder.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SlidesController : ControllerBase
{
    private readonly ISlideEditService _slideEditService;

    public SlidesController(ISlideEditService slideEditService)
    {
        _slideEditService = slideEditService;
    }

    [HttpPost("{id}/regenerate")]
    public async Task<ActionResult<Slide>> RegenerateSlide(Guid id, [FromBody] RegenerateSlideRequest request)
    {
        try
        {
            var slide = await _slideEditService.RegenerateSlideAsync(id, request.Instruction, CancellationToken.None);
            return Ok(slide);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record RegenerateSlideRequest(string? Instruction);
