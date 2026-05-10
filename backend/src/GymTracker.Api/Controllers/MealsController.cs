using GymTracker.Application.Meals;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/meals")]
public sealed class MealsController : ControllerBase
{
    private readonly MealLogService _mealLogService;
    private readonly MealHistoryService _mealHistoryService;

    public MealsController(MealLogService mealLogService, MealHistoryService mealHistoryService)
    {
        _mealLogService = mealLogService;
        _mealHistoryService = mealHistoryService;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IReadOnlyCollection<MealLogResponse>>> GetLogs([FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _mealLogService.ListByDateAsync(userId, date, cancellationToken));
    }

    [HttpPost("logs")]
    public async Task<ActionResult<MealLogResponse>> PostLog([FromBody] CreateMealLogRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var created = await _mealLogService.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetLogs), new { date = created.LoggedDate }, created);
        }
        catch (ArgumentException exception)
        {
            return ValidationProblem(detail: exception.Message);
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<MealHistoryPageResponse>> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _mealHistoryService.GetHistoryAsync(userId, page, pageSize, cancellationToken));
    }

    private string? GetUserId() => HttpContext.Items["CurrentUserId"] as string;
}
