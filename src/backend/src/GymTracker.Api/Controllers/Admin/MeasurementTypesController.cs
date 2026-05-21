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

    [HttpGet]
    public async Task<ActionResult<MeasurementTypesPageResponse>> List(
        [FromQuery] bool includeInactive = false,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string? code = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListPageAsync(
            includeInactive || includeDeleted,
            search,
            code,
            sortBy,
            sortDirection,
            page,
            pageSize,
            cancellationToken);

        return Ok(rows);
    }

    [HttpGet("assignable")]
    public async Task<ActionResult<IReadOnlyCollection<AssignableMeasurementTypeResponse>>> ListAssignable(CancellationToken cancellationToken = default)
    {
        var rows = await _service.ListAssignableAsync(cancellationToken);
        return Ok(rows.ToArray());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MeasurementTypeResponse>> GetById(string id, CancellationToken cancellationToken = default)
    {
        var row = await _service.GetByIdAsync(id, cancellationToken);
        if (row is null)
        {
            return NotFound();
        }

        return Ok(row);
    }

    [HttpPost]
    public async Task<ActionResult<MeasurementTypeResponse>> Create([FromBody] UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var created = await _service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPost("import-csv")]
    public async Task<ActionResult<ImportMeasurementTypesResult>> ImportCsv(
        [FromBody] ImportMeasurementTypesRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ImportCsvAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MeasurementTypeResponse>> Update(string id, [FromBody] UpsertMeasurementTypeRequest request, CancellationToken cancellationToken = default)
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
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id}/reactivate")]
    public async Task<ActionResult<MeasurementTypeResponse>> Reactivate(string id, CancellationToken cancellationToken = default)
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
            return Conflict(new ProblemDetails { Title = "Conflict", Detail = exception.Message });
        }
    }
}
