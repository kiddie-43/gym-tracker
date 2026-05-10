using GymTracker.Application.Catalog;

using Microsoft.AspNetCore.Mvc;

namespace GymTracker.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly CatalogAvailabilityService _catalogAvailabilityService;

    public CatalogController(ICatalogService catalogService, CatalogAvailabilityService catalogAvailabilityService)
    {
        _catalogService = catalogService;
        _catalogAvailabilityService = catalogAvailabilityService;
    }

    [HttpGet("muscle-groups")]
    public async Task<ActionResult<IReadOnlyCollection<MuscleGroupDto>>> GetMuscleGroups(CancellationToken cancellationToken)
    {
        var groups = await _catalogService.GetMuscleGroupsAsync(cancellationToken);
        return Ok(groups);
    }

    [HttpGet("exercises")]
    public async Task<ActionResult<IReadOnlyCollection<ExerciseDto>>> GetExercises(
        [FromQuery] string? query,
        [FromQuery] IReadOnlyCollection<string>? muscleGroupIds,
        CancellationToken cancellationToken)
    {
        var languageCode = ResolveLanguageCodeFromHeader();
        var exercises = await _catalogService.GetExercisesAsync(languageCode, query, muscleGroupIds, cancellationToken);
        return Ok(exercises);
    }

    private string? ResolveLanguageCodeFromHeader()
    {
        var header = Request.Headers.AcceptLanguage.ToString();
        if (string.IsNullOrWhiteSpace(header))
        {
            return null;
        }

        var rawPrimary = header.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawPrimary))
        {
            return null;
        }

        var language = rawPrimary.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];
        var normalized = language.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    [HttpGet("status")]
    public async Task<ActionResult<CatalogAvailabilityDto>> GetStatus(CancellationToken cancellationToken)
    {
        return Ok(await _catalogAvailabilityService.GetAsync(cancellationToken));
    }
}
