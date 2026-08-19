using GymTracker.Application.TrainingSession;
using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TrainingSessionsController : CurrentUserControllerBase
{
    private readonly TrainingSessionService _service;

    public TrainingSessionsController(TrainingSessionService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<TrainingSessionResponse>> Create(
        [FromBody] CreateTrainingSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var created = await _service.CreateAsync(request, userId.Value, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingSessionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var session = await _service.GetByIdAsync(id, userId.Value, cancellationToken);

        return session is null ? NotFound() : Ok(session);
    }

    [HttpGet("week/{weekNumber:int}/day/{dayNumber:int}/exercise/{exerciseId:guid}/history")]
    public async Task<ActionResult<IReadOnlyCollection<TrainingSessionResponse>>> ListHistory(
        [FromRoute] int weekNumber,
        [FromRoute] int dayNumber,
        [FromRoute] Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var sessions = await _service.ListHistoryAsync(userId.Value, weekNumber, dayNumber, exerciseId, cancellationToken);

        return Ok(sessions);
    }

    [HttpGet("block-templates/{exerciseType}")]
    public ActionResult<IReadOnlyCollection<BlockTemplateItemResponse>> GetBlockTemplates(
        [FromRoute] string exerciseType)
    {
        if (!System.Enum.TryParse<GymTracker.Domain.Enum.ExerciseType>(exerciseType, true, out var parsed)
            || !System.Enum.IsDefined(parsed))
        {
            return BadRequest(new { message = $"ExerciseType '{exerciseType}' is invalid." });
        }

        return Ok(BlockTemplateCatalog.GetTemplate(parsed));
    }
}
