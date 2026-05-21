using GymTracker.Application.Contracts.Training;
using GymTracker.Application.Features.Training;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers.Training;

[ApiController]
[Route("api/training-flow")]
public sealed class TrainingFlowController : ControllerBase
{
    private readonly TrainingFlowHandlers _trainingFlowHandlers;

    public TrainingFlowController(TrainingFlowHandlers trainingFlowHandlers)
    {
        _trainingFlowHandlers = trainingFlowHandlers;
    }

    [HttpPost("start")]
    public async Task<ActionResult<TrainingFlowStateDto>> Start([FromBody] StartTrainingFlowDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await _trainingFlowHandlers.StartAsync(userId, request, cancellationToken));
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        await _trainingFlowHandlers.CancelAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<TrainingFlowStateDto?>> Active(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var state = await _trainingFlowHandlers.GetActiveAsync(userId, cancellationToken);
        return Ok(state);
    }

    [HttpPut("active")]
    public async Task<ActionResult<TrainingFlowStateDto>> Update([FromBody] UpdateTrainingFlowDto request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var state = await _trainingFlowHandlers.UpdateAsync(userId, request, cancellationToken);
        return state is null ? NotFound() : Ok(state);
    }

    private string? GetUserId() => HttpContext.Items["CurrentUserId"] as string;
}
