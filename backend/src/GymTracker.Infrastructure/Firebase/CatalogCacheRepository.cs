using GymTracker.Application.Catalog;

namespace GymTracker.Infrastructure.Firebase;

public sealed class CatalogCacheRepository
{
    private readonly FirestoreContext _firestoreContext;

    public CatalogCacheRepository(FirestoreContext firestoreContext)
    {
        _firestoreContext = firestoreContext;
    }

    public Task<IReadOnlyCollection<MuscleGroupDto>?> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
    {
        return _firestoreContext.GetAsync<IReadOnlyCollection<MuscleGroupDto>>(BuildKey("muscle-groups"), cancellationToken);
    }

    public Task SaveMuscleGroupsAsync(IReadOnlyCollection<MuscleGroupDto> items, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.SetAsync(BuildKey("muscle-groups"), items, cancellationToken);
    }

    public Task<IReadOnlyCollection<ExerciseDto>?> GetExercisesAsync(string key, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.GetAsync<IReadOnlyCollection<ExerciseDto>>(BuildKey($"exercises/{key}"), cancellationToken);
    }

    public Task SaveExercisesAsync(string key, IReadOnlyCollection<ExerciseDto> items, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.SetAsync(BuildKey($"exercises/{key}"), items, cancellationToken);
    }

    public Task<IReadOnlyCollection<FoodDto>?> GetFoodsAsync(string key, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.GetAsync<IReadOnlyCollection<FoodDto>>(BuildKey($"foods/{key}"), cancellationToken);
    }

    public Task SaveFoodsAsync(string key, IReadOnlyCollection<FoodDto> items, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.SetAsync(BuildKey($"foods/{key}"), items, cancellationToken);
    }

    public Task<CatalogAvailabilityDto?> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        return _firestoreContext.GetAsync<CatalogAvailabilityDto>(BuildKey("availability"), cancellationToken);
    }

    public Task SaveAvailabilityAsync(CatalogAvailabilityDto availability, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.SetAsync(BuildKey("availability"), availability, cancellationToken);
    }

    private string BuildKey(string documentId)
    {
        return _firestoreContext.BuildCatalogKey("catalog", documentId);
    }
}
