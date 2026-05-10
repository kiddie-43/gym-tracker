using GymTracker.Application.Progress;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/progress")]
public sealed class ProgressController : ControllerBase
{
    private readonly ProgressService _progressService;

    public ProgressController(ProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("exercises/{exerciseId}")]
    public async Task<ActionResult<ProgressResponse>> GetByExercise(string exerciseId, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Items["CurrentUserId"] as string;
        if (userId is null)
        {
            return Unauthorized();
        }

        var response = await _progressService.GetByExerciseAsync(userId, exerciseId, cancellationToken);
        return Ok(response);
    }
}
