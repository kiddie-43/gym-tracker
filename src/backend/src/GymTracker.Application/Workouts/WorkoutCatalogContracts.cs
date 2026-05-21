namespace GymTracker.Application.Workouts;

public sealed record WorkoutCatalogExerciseDto(
    string Id,
    string Name,
    IReadOnlyCollection<string> MuscleGroupIds,
    string? CoverStoragePath,
    string FormTypeId,
    string FormTypeCode);
