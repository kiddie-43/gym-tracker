using GymTracker.Application.Admin.Common;
using GymTracker.Application.Admin.Muscles;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class MusclesController : ControllerBase
{
    private readonly MuscleService _service;

    public MusclesController(MuscleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<MusclesPageResponse>> List(
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
    public async Task<ActionResult<MuscleResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<MuscleResponse>> Create([FromBody] CreateMuscleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MuscleResponse>> Update(Guid id, [FromBody] UpdateMuscleRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, request, cancellationToken);
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(updated);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult<MuscleResponse>> Reactivate(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _service.ReactivateAsync(id, cancellationToken);
            if (row is null)
            {
                return NotFound();
            }

            return Ok(row);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPost("import-csv")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImportMusclesResult>> ImportCsv(
    IFormFile file,
    CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
            return BadRequest("CSV file is required.");

        await using var stream = file.OpenReadStream();

        var result = await _service.ImportCsvAsync(stream, cancellationToken);

        return Ok(result);
    }
}

public sealed record ImportMusclesCsvRequest(
    IReadOnlyCollection<ImportMuscleCsvRowRequest>? Rows);

public sealed record ImportMuscleCsvRowRequest(
    string? Name,
    string? Code,
    string? Description);
