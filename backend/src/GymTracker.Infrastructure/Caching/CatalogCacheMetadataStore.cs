using GymTracker.Application.Catalog;

namespace GymTracker.Infrastructure.Caching;

public sealed class CatalogCacheMetadataStore
    : ICatalogAvailabilityStore
{
    private CatalogAvailabilityDto _state = new(true, DateTimeOffset.MinValue);

    public Task<CatalogAvailabilityDto> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_state);
    }

    public Task MarkHealthyAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state = new CatalogAvailabilityDto(false, updatedAt);
        return Task.CompletedTask;
    }

    public Task MarkDegradedAsync(DateTimeOffset updatedAt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _state = new CatalogAvailabilityDto(true, updatedAt);
        return Task.CompletedTask;
    }
}
