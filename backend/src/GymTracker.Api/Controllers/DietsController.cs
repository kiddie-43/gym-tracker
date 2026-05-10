using GymTracker.Application.Diets;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/diets")]
public sealed class DietsController : ControllerBase
{
    private readonly DietService _dietService;

    public DietsController(DietService dietService)
    {
        _dietService = dietService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<DietSummaryResponse>>> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _dietService.ListAsync(userId, cancellationToken));
    }

    [HttpGet("{dietId}")]
    public async Task<ActionResult<DietResponse>> GetById(string dietId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var diet = await _dietService.GetByIdAsync(userId, dietId, cancellationToken);
        return diet is null ? NotFound() : Ok(diet);
    }

    [HttpPost]
    public async Task<ActionResult<DietResponse>> Post([FromBody] CreateDietRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var created = await _dietService.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { dietId = created.Id }, created);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPut("{dietId}")]
    public async Task<ActionResult<DietResponse>> Put(string dietId, [FromBody] CreateDietRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var updated = await _dietService.UpdateAsync(userId, dietId, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpDelete("{dietId}")]
    public async Task<IActionResult> Delete(string dietId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _dietService.DeleteAsync(userId, dietId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    private string? GetUserId() => HttpContext.Items["CurrentUserId"] as string;
}
