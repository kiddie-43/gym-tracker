using GymTracker.Application.Settings;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly UserPreferencesService _userPreferencesService;

    public SettingsController(UserPreferencesService userPreferencesService)
    {
        _userPreferencesService = userPreferencesService;
    }

    [HttpGet("preferences")]
    public async Task<ActionResult<PreferencesResponse>> GetPreferences(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _userPreferencesService.GetAsync(userId, cancellationToken));
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<PreferencesResponse>> PutPreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        return Ok(await _userPreferencesService.UpdateAsync(userId, request, cancellationToken));
    }

    private string? GetUserId() => HttpContext.Items["CurrentUserId"] as string;
}
