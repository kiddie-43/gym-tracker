using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExercicesController : ControllerBase
{
    private readonly ExerciceService _service;

    public ExercicesController(ExerciceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ExercicePageResponse>> List(
        [FromQuery] string? code = null,
        [FromQuery] string? name = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListPageAsync(code, name, sortBy, sortDirection, page, pageSize, cancellationToken);
        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExerciceResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<ExerciceResponse>> Create([FromBody] CreateExerciceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExerciceResponse>> Update(Guid id, [FromBody] UpdateExerciceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<ExerciceResponse>> Reactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _service.ReactivateAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost("import-csv")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportExercicesResult>> ImportCsv(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("CSV file is required.");
        }

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportCsvAsync(stream, cancellationToken);

        return Ok(result);
    }
}
