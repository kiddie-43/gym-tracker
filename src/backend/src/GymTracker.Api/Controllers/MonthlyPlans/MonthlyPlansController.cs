using GymTracker.Application.MonthlyPlan;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/monthly-plans")]
public sealed class MonthlyPlansController : CurrentUserControllerBase
{
    private readonly MonthlyPlanService _service;
    private readonly ILogger<MonthlyPlansController> _logger;

    public MonthlyPlansController(MonthlyPlanService service, ILogger<MonthlyPlansController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<MonthlyPlanResponse>> GetMine(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var plan = await _service.GetOrCreateAsync(userId.Value, cancellationToken);
            _logger.LogInformation("Monthly plan loaded for user {UserId}", userId.Value);
            return Ok(plan);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Monthly plan unavailable for user {UserId}", userId.Value);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Monthly plan temporarily unavailable." });
        }
    }

    [HttpPut("mine")]
    public async Task<ActionResult<MonthlyPlanResponse>> UpsertMine([FromBody] UpsertMonthlyPlanRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var plan = await _service.UpsertAsync(userId.Value, request, cancellationToken);
            _logger.LogInformation("Monthly plan updated for user {UserId} with active days {ActiveDays}", userId.Value, request.ActiveDays);
            return Ok(plan);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Invalid monthly plan update payload for user {UserId}", userId.Value);
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Monthly plan save unavailable for user {UserId}", userId.Value);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Monthly plan save temporarily unavailable." });
        }
    }

    [HttpPost("mine/truncate-days")]
    public async Task<ActionResult<MonthlyPlanResponse>> TruncateDays([FromBody] TruncateDaysRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var plan = await _service.TruncateDaysAsync(userId.Value, request, cancellationToken);
            _logger.LogInformation("Monthly plan truncated for user {UserId} to active days {ActiveDays}", userId.Value, request.ActiveDays);
            return Ok(plan);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Invalid monthly truncation payload for user {UserId}", userId.Value);
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Monthly plan truncation unavailable for user {UserId}", userId.Value);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "Monthly plan truncation temporarily unavailable." });
        }
    }

    [HttpPost("mine/exercises")]
    public async Task<ActionResult> LinkExerciseToDay([FromBody] LinkExerciseToDayRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var id = await _service.LinkExerciseToDayAsync(userId.Value, request, cancellationToken);
            _logger.LogInformation(
                "Exercise linked to monthly plan for user {UserId}. Week={WeekId} Day={DayId} Exercise={ExerciseId}",
                userId.Value,
                request.WeekId,
                request.DayId,
                request.ExerciseId);
            return StatusCode(StatusCodes.Status201Created, new { id, message = "Created" });
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Invalid monthly plan link payload for user {UserId}", userId.Value);
            return BadRequest(new { message = exception.Message });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Monthly plan link unavailable for user {UserId}", userId.Value);
            var cause = exception.GetBaseException().Message;
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Monthly plan link failed.", cause });
        }
    }

    [HttpPatch("mine/exercises/{id:guid}")]
    public async Task<ActionResult> UpdatePlannedExercise(Guid id, [FromBody] UpdatePlannedExerciseRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _service.UpdatePlannedExerciseAsync(userId.Value, id, request, cancellationToken);
            _logger.LogInformation("Planned exercise {Id} updated for user {UserId}", id, userId.Value);
            return Ok(new { message = "Updated" });
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(exception, "Invalid planned exercise update payload for user {UserId}", userId.Value);
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Planned exercise not found." });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Planned exercise update unavailable for user {UserId}", userId.Value);
            var cause = exception.GetBaseException().Message;
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Planned exercise update failed.", cause });
        }
    }

    [HttpDelete("mine/exercises/{id:guid}")]
    public async Task<ActionResult> UnlinkExercise(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _service.UnlinkExerciseAsync(userId.Value, id, cancellationToken);
            _logger.LogInformation("Planned exercise {Id} unlinked for user {UserId}", id, userId.Value);
            return Ok(new { message = "Deleted" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Planned exercise not found." });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Planned exercise unlink unavailable for user {UserId}", userId.Value);
            var cause = exception.GetBaseException().Message;
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Planned exercise unlink failed.", cause });
        }
    }

}

