using GymTracker.Application.Admin.MeasurementTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Admin;

[ApiController]
[Route(AdminRoutes.MeasurementTypes)]
[AllowAnonymous]
public sealed class MeasurementTypesController : ControllerBase
{
    private readonly MeasurementTypeService _service;

    public MeasurementTypesController(MeasurementTypeService service)
    {
        _service = service;
    }

    // GET api/admin/measurement-types
    [HttpGet]
    public async Task<ActionResult<MeasurementTypesPageResponse>> List(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string? code = null,
        [FromQuery] string sortBy = "code",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListPageAsync(
            includeDeleted,
            search?.Trim(),
            code?.Trim(),
            sortBy,
            sortDirection,
            page,
            pageSize,
            cancellationToken);

        return Ok(result);
    }

    // GET api/admin/measurement-types/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<MeasurementTypeResponse>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        return row is null ? NotFound() : Ok(row);
    }

    // POST api/admin/measurement-types
    [HttpPost]
    public async Task<ActionResult<MeasurementTypeResponse>> Create(
        [FromBody] UpsertMeasurementTypeRequest request,
        CancellationToken cancellationToken = default)
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

    // PUT api/admin/measurement-types/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult<MeasurementTypeResponse>> Update(
        string id,
        [FromBody] UpsertMeasurementTypeRequest request,
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

    // DELETE api/admin/measurement-types/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // GET api/admin/measurement-types/search?q=texto
    // Usado por otros módulos (ej. Exercises) para alimentar selectores
    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyCollection<AssignableMeasurementTypeResponse>>> Search(
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.SearchAsync(q?.Trim(), cancellationToken);
        return Ok(rows);
    }

    // POST api/admin/measurement-types/import-csv
    [HttpPost("import-csv")]
    public async Task<ActionResult<ImportMeasurementTypesResult>> ImportCsv(
        [FromBody] ImportMeasurementTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ImportCsvAsync(request, cancellationToken);
        return Ok(result);
    }

    // POST api/admin/measurement-types/{id}/reactivate
    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<MeasurementTypeResponse>> Reactivate(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _service.ReactivateAsync(id, cancellationToken);
            return row is null ? NotFound() : Ok(row);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}

