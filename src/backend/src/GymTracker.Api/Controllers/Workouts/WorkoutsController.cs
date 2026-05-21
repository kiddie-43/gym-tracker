using GymTracker.Application.Workouts;
using GymTracker.Infrastructure.Firebase;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Workouts;

[ApiController]
[Route("api/workouts")]
public sealed class WorkoutsController : ControllerBase
{
    private readonly WorkoutService _workoutService;
    private readonly WorkoutHistoryService _workoutHistoryService;
    private readonly DashboardSummaryRepository _dashboardSummaryRepository;
    private readonly WorkoutCatalogService _workoutCatalogService;

    public WorkoutsController(
        WorkoutService workoutService,
        WorkoutHistoryService workoutHistoryService,
        DashboardSummaryRepository dashboardSummaryRepository,
        WorkoutCatalogService workoutCatalogService)
    {
        _workoutService = workoutService;
        _workoutHistoryService = workoutHistoryService;
        _dashboardSummaryRepository = dashboardSummaryRepository;
        _workoutCatalogService = workoutCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<WorkoutHistoryPageResponse>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _workoutHistoryService.GetHistoryAsync(userId, from, to, page, pageSize, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<WorkoutResponse>> Post([FromBody] CreateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _workoutService.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkoutResponse>> GetById(string id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _workoutService.GetByIdAsync(userId, id, cancellationToken);
        if (response is null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WorkoutResponse>> Put(string id, [FromBody] UpdateWorkoutRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var response = await _workoutService.UpdateAsync(userId, id, request, cancellationToken);
            return Ok(response);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _workoutService.DeleteAsync(userId, id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    private string? GetUserId()
    {
        return HttpContext.Items["CurrentUserId"] as string;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummary>> GetSummary(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _dashboardSummaryRepository.GetSummaryAsync(userId, cancellationToken));
    }

    [HttpGet("exercise-catalog")]
    public async Task<ActionResult<IReadOnlyCollection<WorkoutCatalogExerciseDto>>> GetExerciseCatalog(
        [FromQuery] string? query,
        [FromQuery] IReadOnlyCollection<string>? muscleGroupIds,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _workoutCatalogService.ListAsync(query, muscleGroupIds, cancellationToken));
    }
}
