using GymTracker.Application.Admin.Common;
using GymTracker.Application.Admin.Exercises;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Admin;

[ApiController]
[Route(AdminRoutes.Exercises)]
public sealed class ExercisesController : ControllerBase
{
    private readonly ExerciseService _service;

    public ExercisesController(ExerciseService service)
    {
        _service = service;
    }

    // 1. GET / — listado paginado
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortDirection = "asc",
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListPageAsync(
            includeDeleted, search, category, difficulty,
            sortBy, sortDirection, page, pageSize, cancellationToken);
        return Ok(result);
    }

    // 2. GET /{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken = default)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    // 3. POST /
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] UpsertExerciseItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _service.CreateAsync(Map(request), cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 4. PUT /{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpsertExerciseItemRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _service.UpdateAsync(id, Map(request), cancellationToken);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 5. DELETE /{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    // 6. GET /search?q=
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _service.SearchAsync(q, cancellationToken);
        return Ok(items);
    }

    // POST /{id}/reactivate
    [HttpPost("{id}/reactivate")]
    public async Task<IActionResult> Reactivate(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _service.ReactivateAsync(id, cancellationToken);
            return dto is null ? NotFound() : Ok(dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private static UpsertExerciseRequest Map(UpsertExerciseItemRequest r) => new(
        Name: AdminRequestSanitizer.RequiredTrimmed(r.Name, nameof(r.Name)),
        Code: AdminRequestSanitizer.OptionalTrimmed(r.Code),
        Category: AdminRequestSanitizer.RequiredTrimmed(r.Category, nameof(r.Category)),
        Difficulty: AdminRequestSanitizer.RequiredTrimmed(r.Difficulty, nameof(r.Difficulty)),
        PrimaryMuscleIds: r.PrimaryMuscleIds ?? Array.Empty<string>(),
        SecondaryMuscleIds: r.SecondaryMuscleIds ?? Array.Empty<string>(),
        MeasurementTypeIds: r.MeasurementTypeIds ?? Array.Empty<string>(),
        Active: r.Active ?? true);
}

public sealed record UpsertExerciseItemRequest(
    string Name,
    string? Code,
    string Category,
    string Difficulty,
    IReadOnlyCollection<string>? PrimaryMuscleIds,
    IReadOnlyCollection<string>? SecondaryMuscleIds,
    IReadOnlyCollection<string>? MeasurementTypeIds,
    bool? Active);

