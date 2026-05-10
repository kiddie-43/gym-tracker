namespace GymTracker.Application.Catalog;

public sealed class CatalogService : ICatalogService
{
    private readonly ICatalogDataSource _catalogDataSource;

    public CatalogService(ICatalogDataSource catalogDataSource)
    {
        _catalogDataSource = catalogDataSource;
    }

    public Task<IReadOnlyCollection<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default)
    {
        return _catalogDataSource.GetMuscleGroupsAsync(cancellationToken);
    }

    public Task<IReadOnlyCollection<ExerciseDto>> GetExercisesAsync(string? languageCode, string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default)
    {
        return _catalogDataSource.GetExercisesAsync(languageCode, query, muscleGroupIds, cancellationToken);
    }

    public Task<IReadOnlyCollection<FoodDto>> GetFoodsAsync(string query, CancellationToken cancellationToken = default)
    {
        return _catalogDataSource.GetFoodsAsync(query, cancellationToken);
    }

    public Task<CatalogAvailabilityDto> GetAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        return _catalogDataSource.GetAvailabilityAsync(cancellationToken);
    }
}
