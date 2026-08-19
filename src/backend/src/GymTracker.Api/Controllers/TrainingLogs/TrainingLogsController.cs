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

    [HttpPut("{groupId:guid}")]
    public async Task<ActionResult<TrainingLogGroupResponse>> UpdateGroup(
        Guid groupId,
        [FromBody] UpdateTrainingLogValueRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var updated = await _service.UpdateValueAsync(groupId, request, userId.Value, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> Delete(
        Guid groupId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var deleted = await _service.DeleteAsync(groupId, userId.Value, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }


    [HttpGet("/api/[controller]/week/{weekNumber:int}/day/{dayNumber:int}/exercise/{exerciseCode}/logs")]
    public async Task<ActionResult<IReadOnlyCollection<TrainingLogGroupResponse>>> List(
        [FromRoute] int weekNumber,
        [FromRoute] int dayNumber,
        [FromRoute] string exerciseCode,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        if (!userId.HasValue)
            return Unauthorized();

        var rows = await _service.ListAsync(
            userId.Value,
            weekNumber,
            dayNumber,
            exerciseCode,
            cancellationToken);

        return Ok(rows);
    }
}