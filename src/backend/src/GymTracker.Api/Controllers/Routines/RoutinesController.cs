using GymTracker.Application.Contracts.Routines;
using GymTracker.Application.Features.Routines;
using GymTracker.Application.Catalog;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Routines;

[ApiController]
[Route("api/routines")]
public sealed class RoutinesController : ControllerBase
{
    private readonly RoutinesCommandHandlers _routines;
    private readonly RoutineSessionsCommandHandlers _sessions;
    private readonly SessionExercisesHandlers _sessionExercises;
    private readonly PlannedSetsHandlers _plannedSets;
    private readonly ExerciseCatalogFallbackService _catalogFallback;

    public RoutinesController(
        RoutinesCommandHandlers routines,
        RoutineSessionsCommandHandlers sessions,
        SessionExercisesHandlers sessionExercises,
        PlannedSetsHandlers plannedSets,
        ExerciseCatalogFallbackService catalogFallback)
    {
        _routines = routines;
        _sessions = sessions;
        _sessionExercises = sessionExercises;
        _plannedSets = plannedSets;
        _catalogFallback = catalogFallback;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoutineCardDto>>> Get([FromQuery] bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var routines = await _routines.ListAsync(userId, includeDeleted, cancellationToken);
        return Ok(routines);
    }

    [HttpPost]
    public async Task<ActionResult<RoutineDetailDto>> Post([FromBody] CreateRoutineDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var created = await _routines.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { routineId = created.Id }, created);
    }

    [HttpGet("{routineId}")]
    public async Task<ActionResult<RoutineDetailDto>> GetById(string routineId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var routine = await _routines.GetByIdAsync(userId, routineId, cancellationToken);
        if (routine is null)
        {
            return NotFound();
        }

        return Ok(routine);
    }

    [HttpPatch("{routineId}")]
    public async Task<ActionResult<RoutineDetailDto>> Patch(string routineId, [FromBody] UpdateRoutineDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _routines.UpdateAsync(userId, routineId, request, cancellationToken);
        if (updated is null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{routineId}")]
    public async Task<IActionResult> Delete(string routineId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _routines.ArchiveAsync(userId, routineId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{routineId}/reactivate")]
    public async Task<ActionResult<RoutineDetailDto>> Reactivate(string routineId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var routine = await _routines.ReactivateAsync(userId, routineId, cancellationToken);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPost("{routineId}/sessions")]
    public async Task<ActionResult<RoutineSessionDto>> CreateSession(string routineId, [FromBody] CreateRoutineSessionDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var session = await _sessions.CreateAsync(userId, routineId, request, cancellationToken);
            return session is null ? NotFound() : StatusCode(StatusCodes.Status201Created, session);
        }
        catch (InvalidOperationException)
        {
            return Conflict();
        }
    }

    [HttpPost("{routineId}/sessions/{sessionId}/exercises")]
    public async Task<ActionResult<SessionExerciseDto>> AddExercise(
        string routineId,
        string sessionId,
        [FromBody] AddSessionExerciseDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var exercise = await _sessionExercises.AddAsync(userId, routineId, sessionId, request, cancellationToken);
        return exercise is null ? NotFound() : StatusCode(StatusCodes.Status201Created, exercise);
    }

    [HttpDelete("{routineId}/sessions/{sessionId}/exercises/{exerciseId}")]
    public async Task<IActionResult> RemoveExercise(string routineId, string sessionId, string exerciseId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var removed = await _sessionExercises.UnlinkAsync(userId, routineId, sessionId, exerciseId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }

    [HttpPatch("{routineId}/sessions/{sessionId}/exercises/{exerciseId}/planned-sets/{setId}")]
    public async Task<ActionResult<PlannedSetDto>> UpdatePlannedSet(
        string routineId,
        string sessionId,
        string exerciseId,
        string setId,
        [FromBody] UpdatePlannedSetDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var updated = await _plannedSets.UpdateAsync(userId, routineId, sessionId, exerciseId, setId, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{routineId}/sessions/{sessionId}/exercises/{exerciseId}/planned-sets/{setId}")]
    public async Task<IActionResult> DeletePlannedSet(
        string routineId,
        string sessionId,
        string exerciseId,
        string setId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _plannedSets.DeleteAsync(userId, routineId, sessionId, exerciseId, setId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("catalog/availability")]
    public async Task<ActionResult<CatalogAvailabilityDto>> GetCatalogAvailability(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _catalogFallback.GetAvailabilityAsync(cancellationToken));
    }

    private string? GetUserId()
    {
#if DEBUG
        return "test-user"; // Usuario de prueba en entorno de desarrollo
#endif
        return HttpContext.Items["CurrentUserId"] as string;
    }
}
