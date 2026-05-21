using Microsoft.Extensions.Caching.Memory;

using GymTracker.Application.Catalog;
using GymTracker.Infrastructure.Firebase;
using GymTracker.Infrastructure.Wger;

namespace GymTracker.Infrastructure.Caching;

public sealed class CatalogCacheService : ICatalogDataSource
{
    private static readonly TimeSpan MemoryTtl = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _memoryCache;
    private readonly CatalogCacheRepository _repository;
    private readonly WgerApiClient _wgerApiClient;
    private readonly CatalogCacheMetadataStore _metadataStore;

    public CatalogCacheService(IMemoryCache memoryCache, CatalogCacheRepository repository, WgerApiClient wgerApiClient, CatalogCacheMetadataStore metadataStore)
    {
        _memoryCache = memoryCache;
        _repository = repository;
        _wgerApiClient = wgerApiClient;
        _metadataStore = metadataStore;
    }

    public async Task<IReadOnlyCollection<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
    {
        return await _memoryCache.GetOrCreateAsync("catalog:muscle-groups", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MemoryTtl;

            var cached = await _repository.GetMuscleGroupsAsync(cancellationToken);
            if (cached is { Count: > 0 })
            {
                return cached;
            }

            try
            {
                var fetched = await _wgerApiClient.GetMuscleGroupsAsync(cancellationToken);
                await _repository.SaveMuscleGroupsAsync(fetched, cancellationToken);
                await _repository.SaveAvailabilityAsync(new CatalogAvailabilityDto(false, DateTimeOffset.UtcNow), cancellationToken);
                await _metadataStore.MarkHealthyAsync(DateTimeOffset.UtcNow, cancellationToken);
                return fetched;
            }
            catch
            {
                await _metadataStore.MarkDegradedAsync(DateTimeOffset.UtcNow, cancellationToken);
                return cached;
            }
        }) ?? Array.Empty<MuscleGroupDto>();
    }

    public async Task<IReadOnlyCollection<ExerciseDto>> GetExercisesAsync(string? languageCode, string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default)
    {
        var key = BuildCacheKey("exercises", languageCode, query, muscleGroupIds);

        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MemoryTtl;

            var cached = await _repository.GetExercisesAsync(key, cancellationToken);
            if (cached is { Count: > 0 })
            {
                return cached;
            }

            try
            {
                var fetched = await _wgerApiClient.GetExercisesAsync(languageCode, query, muscleGroupIds, cancellationToken);
                await _repository.SaveExercisesAsync(key, fetched, cancellationToken);
                await _repository.SaveAvailabilityAsync(new CatalogAvailabilityDto(false, DateTimeOffset.UtcNow), cancellationToken);
                await _metadataStore.MarkHealthyAsync(DateTimeOffset.UtcNow, cancellationToken);
                return fetched;
            }
            catch
            {
                await _metadataStore.MarkDegradedAsync(DateTimeOffset.UtcNow, cancellationToken);
                return cached;
            }
        }) ?? Array.Empty<ExerciseDto>();
    }

    public async Task<IReadOnlyCollection<FoodDto>> GetFoodsAsync(string query, CancellationToken cancellationToken = default)
    {
        var key = BuildCacheKey("foods", null, query, null);

        return await _memoryCache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = MemoryTtl;

            var cached = await _repository.GetFoodsAsync(key, cancellationToken);
            if (cached is { Count: > 0 })
            {
                return cached;
            }

            try
            {
                var fetched = await _wgerApiClient.GetFoodsAsync(query, cancellationToken);
                await _repository.SaveFoodsAsync(key, fetched, cancellationToken);
                await _repository.SaveAvailabilityAsync(new CatalogAvailabilityDto(false, DateTimeOffset.UtcNow), cancellationToken);
                await _metadataStore.MarkHealthyAsync(DateTimeOffset.UtcNow, cancellationToken);
                return fetched;
            }
            catch
            {
                await _metadataStore.MarkDegradedAsync(DateTimeOffset.UtcNow, cancellationToken);
                return cached;
            }
        }) ?? Array.Empty<FoodDto>();
    }

    public async Task<CatalogAvailabilityDto> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        var storeAvailability = await _metadataStore.GetAsync(cancellationToken);
        var repositoryAvailability = await _repository.GetAvailabilityAsync(cancellationToken);

        if (repositoryAvailability is null)
        {
            return storeAvailability;
        }

        return repositoryAvailability.LastUpdatedAt > storeAvailability.LastUpdatedAt
            ? repositoryAvailability
            : storeAvailability;
    }

    private static string BuildCacheKey(string prefix, string? languageCode, string? query, IReadOnlyCollection<string>? values)
    {
        var suffix = values is { Count: > 0 }
            ? string.Join('-', values.OrderBy(value => value, StringComparer.Ordinal))
            : "all";

        var normalizedLanguage = string.IsNullOrWhiteSpace(languageCode) ? "default" : languageCode.Trim().ToLowerInvariant();
        var normalizedQuery = string.IsNullOrWhiteSpace(query) ? "all" : query.Trim().ToLowerInvariant();
        return $"catalog:{prefix}:{normalizedLanguage}:{normalizedQuery}:{suffix}";
    }
}
