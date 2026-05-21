namespace GymTracker.Application.Catalog;

public interface ICatalogAvailabilityStore
{
    Task<CatalogAvailabilityDto> GetAsync(CancellationToken cancellationToken = default);

    Task MarkHealthyAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default);

    Task MarkDegradedAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default);
}
