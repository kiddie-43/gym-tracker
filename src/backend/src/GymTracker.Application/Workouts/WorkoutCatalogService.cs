using GymTracker.Application.Admin.Exercises;

namespace GymTracker.Application.Workouts;

public sealed class WorkoutCatalogService
{
    private readonly ExerciseService _exerciseService;

    public WorkoutCatalogService(ExerciseService exerciseService)
    {
        _exerciseService = exerciseService;
    }

    public async Task<IReadOnlyCollection<WorkoutCatalogExerciseDto>> ListAsync(
        string? query,
        IReadOnlyCollection<string>? muscleGroupIds,
        CancellationToken cancellationToken = default)
    {
        var rows = await _exerciseService.ListWorkoutCatalogAsync(query, muscleGroupIds, cancellationToken);

        return rows
            .Select(row => new WorkoutCatalogExerciseDto(
                row.Id,
                row.Name,
                row.MuscleGroupIds,
                row.CoverStoragePath,
                row.FormTypeId,
                row.FormTypeCode))
            .ToArray();
    }
}
