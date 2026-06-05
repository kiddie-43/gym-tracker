using GymTracker.Application.TrainingLog;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TrainingLogsController : CurrentUserControllerBase
{
    private readonly TrainingLogService _service;

    public TrainingLogsController(TrainingLogService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<TrainingLogGroupResponse>> Create(
        [FromBody] CreateTrainingEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var created = await _service.CreateAsync(request, userId.Value, cancellationToken);
        return Ok(created);


    }
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingLogResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var log = await _service.GetByIdAsync(id, cancellationToken);

        return log is null
            ? NotFound()
            : Ok(log);
    }


    [HttpGet("/api/[controller]/routine/{routineId}/session/{sessionId}/exercise/{exerciseId}/logs")]
    public async Task<ActionResult<IReadOnlyCollection<TrainingLogGroupResponse>>> List(
        [FromRoute] string routineId,
        [FromRoute] string sessionId,
        [FromRoute] string exerciseId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
            return Unauthorized();

        var rows = await _service.ListAsync(
            userId.Value,
            routineId,
            sessionId,
            exerciseId,
            cancellationToken);

        return Ok(rows);
    }
}