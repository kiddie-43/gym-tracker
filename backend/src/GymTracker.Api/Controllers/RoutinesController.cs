using GymTracker.Application.Routines;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/routines")]
public sealed class RoutinesController : ControllerBase
{
    private readonly RoutineService _routineService;

    public RoutinesController(RoutineService routineService)
    {
        _routineService = routineService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<RoutineSummaryResponse>>> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var routines = await _routineService.ListAsync(userId, cancellationToken);
        return Ok(routines);
    }

    [HttpGet("{routineId}")]
    public async Task<ActionResult<RoutineResponse>> GetById(string routineId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var routine = await _routineService.GetByIdAsync(userId, routineId, cancellationToken);
        return routine is null ? NotFound() : Ok(routine);
    }

    [HttpPost]
    public async Task<ActionResult<RoutineResponse>> Post([FromBody] CreateRoutineRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _routineService.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { routineId = response.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpPut("{routineId}")]
    public async Task<ActionResult<RoutineResponse>> Put(string routineId, [FromBody] CreateRoutineRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _routineService.UpdateAsync(userId, routineId, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpDelete("{routineId}")]
    public async Task<IActionResult> Delete(string routineId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _routineService.DeleteAsync(userId, routineId, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("suggested-exercises")]
    public async Task<ActionResult<SuggestedExercisesResponse>> GetSuggestedExercises(
        [FromQuery] IReadOnlyCollection<string> muscleGroupIds,
        [FromQuery] string? query,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _routineService.GetSuggestedExercisesAsync(muscleGroupIds, query, cancellationToken);
        return Ok(response);
    }

    private string? GetUserId()
    {
        return HttpContext.Items["CurrentUserId"] as string;
    }
}
