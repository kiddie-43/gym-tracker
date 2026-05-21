using GymTracker.Application.Catalog;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class CatalogCacheRepository
{
    private const string ScopeName = "catalog";
    private readonly SqlDocumentStore _store;

    public CatalogCacheRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyCollection<MuscleGroupDto>?> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
    {
        return _store.GetDocumentAsync<IReadOnlyCollection<MuscleGroupDto>>(ScopeName, BuildKey("muscle-groups"), cancellationToken);
    }

    public Task SaveMuscleGroupsAsync(IReadOnlyCollection<MuscleGroupDto> items, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync(ScopeName, BuildKey("muscle-groups"), items, cancellationToken);
    }

    public Task<IReadOnlyCollection<ExerciseDto>?> GetExercisesAsync(string key, CancellationToken cancellationToken = default)
    {
        return _store.GetDocumentAsync<IReadOnlyCollection<ExerciseDto>>(ScopeName, BuildKey($"exercises/{key}"), cancellationToken);
    }

    public Task SaveExercisesAsync(string key, IReadOnlyCollection<ExerciseDto> items, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync(ScopeName, BuildKey($"exercises/{key}"), items, cancellationToken);
    }

    public Task<IReadOnlyCollection<FoodDto>?> GetFoodsAsync(string key, CancellationToken cancellationToken = default)
    {
        return _store.GetDocumentAsync<IReadOnlyCollection<FoodDto>>(ScopeName, BuildKey($"foods/{key}"), cancellationToken);
    }

    public Task SaveFoodsAsync(string key, IReadOnlyCollection<FoodDto> items, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync(ScopeName, BuildKey($"foods/{key}"), items, cancellationToken);
    }

    public Task<CatalogAvailabilityDto?> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        return _store.GetDocumentAsync<CatalogAvailabilityDto>(ScopeName, BuildKey("availability"), cancellationToken);
    }

    public Task SaveAvailabilityAsync(CatalogAvailabilityDto availability, CancellationToken cancellationToken = default)
    {
        return _store.UpsertDocumentAsync(ScopeName, BuildKey("availability"), availability, cancellationToken);
    }

    private string BuildKey(string documentId)
    {
        return documentId;
    }
}
