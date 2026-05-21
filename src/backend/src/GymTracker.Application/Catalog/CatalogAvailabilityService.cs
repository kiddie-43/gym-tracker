namespace GymTracker.Application.Catalog;

public sealed class CatalogAvailabilityService
{
    private readonly ICatalogAvailabilityStore _metadataStore;

    public CatalogAvailabilityService(ICatalogAvailabilityStore metadataStore)
    {
        _metadataStore = metadataStore;
    }

    public Task<CatalogAvailabilityDto> GetAsync(CancellationToken cancellationToken = default)
    {
        return _metadataStore.GetAsync(cancellationToken);
    }
}
