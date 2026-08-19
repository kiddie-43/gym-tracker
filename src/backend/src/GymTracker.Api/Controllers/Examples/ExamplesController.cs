using GymTracker.Application.Examples;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ExamplesController : ControllerBase
{
    private readonly ExampleService _service;

    public ExamplesController(ExampleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ExampleResponse>>> List(CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListAsync(cancellationToken);
        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExampleResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<ExampleResponse>> Create([FromBody] UpsertExampleRequest request, CancellationToken cancellationToken = default)
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
    public async Task<ActionResult<ExampleResponse>> Update(
        Guid id,
        [FromBody] UpsertExampleRequest request,
        CancellationToken cancellationToken = default)
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
}