using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Features.Training;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Training;

[ApiController]
[Route("api/exercise-training-logs")]
public sealed class ExerciseTrainingLogsController : ControllerBase
{
    private readonly ExerciseTrainingLogHandlers _handlers;

    public ExerciseTrainingLogsController(ExerciseTrainingLogHandlers handlers)
    {
        _handlers = handlers;
    }

    [HttpPost]
    public async Task<ActionResult<ExerciseTrainingLogDto>> Post([FromBody] CreateExerciseTrainingLogDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var created = await _handlers.SaveAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { logId = created.Id }, created);
    }

    [HttpGet("{logId}")]
    public async Task<ActionResult<ExerciseTrainingLogDto>> GetById(string logId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var log = await _handlers.GetAsync(userId, logId, cancellationToken);
        return log is null ? NotFound() : Ok(log);
    }

    [HttpGet("progress-comparison")]
    public async Task<ActionResult<ProgressComparisonDto>> ProgressComparison([FromQuery] string exerciseId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _handlers.GetProgressComparisonAsync(userId, exerciseId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    private string? GetUserId() => HttpContext.Items["CurrentUserId"] as string;
}
