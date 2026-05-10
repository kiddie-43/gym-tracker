namespace GymTracker.Application.Catalog;

public interface ICatalogDataSource
{
    Task<IReadOnlyCollection<MuscleGroupDto>> GetMuscleGroupsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExerciseDto>> GetExercisesAsync(string? languageCode, string? query, IReadOnlyCollection<string>? muscleGroupIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FoodDto>> GetFoodsAsync(string query, CancellationToken cancellationToken = default);

    Task<CatalogAvailabilityDto> GetAvailabilityAsync(CancellationToken cancellationToken = default);
}
