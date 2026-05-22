namespace GymTracker.Application.Admin.Exercises;

public sealed record ExerciseCatalogItem(
    string Id,
    string Name,
    IReadOnlyCollection<string> MuscleGroupIds,
    string? CoverStoragePath,
    string FormTypeId,
    string FormTypeCode);
