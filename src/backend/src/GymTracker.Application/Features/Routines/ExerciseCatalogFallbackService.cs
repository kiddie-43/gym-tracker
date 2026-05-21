using GymTracker.Application.Catalog;

namespace GymTracker.Application.Features.Routines;

public sealed class ExerciseCatalogFallbackService
{
    private readonly ICatalogService _catalogService;

    public ExerciseCatalogFallbackService(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    public Task<CatalogAvailabilityDto> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        return _catalogService.GetAvailabilityAsync(cancellationToken);
    }
}
