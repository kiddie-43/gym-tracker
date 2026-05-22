namespace GymTracker.Application.Admin.Exercises;

public sealed record UpsertExerciseRequest(
    string Name,
    string? Code,
    string Category,
    string Difficulty,
    IReadOnlyCollection<string> PrimaryMuscleIds,
    IReadOnlyCollection<string> SecondaryMuscleIds,
    IReadOnlyCollection<string> MeasurementTypeIds,
    bool Active = true);

public sealed record ExerciseDto(
    string Id,
    string Name,
    string? Code,
    string Category,
    string Difficulty,
    IReadOnlyCollection<ExerciseMuscleDto> PrimaryMuscles,
    IReadOnlyCollection<ExerciseMuscleDto> SecondaryMuscles,
    IReadOnlyCollection<string> PrimaryMuscleIds,
    IReadOnlyCollection<string> SecondaryMuscleIds,
    IReadOnlyCollection<string> MeasurementTypeIds,
    IReadOnlyCollection<string> MeasurementTypeNames,
    bool Active,
    bool IsDeleted,
    DateTimeOffset? DeletedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ExerciseMuscleDto(
    string Id,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<string> MuscleGroupIds,
    bool Active,
    bool IsDeleted);

public sealed record ExercisesPageResponse(
    IReadOnlyCollection<ExerciseDto> Items,
    int Total,
    int Page,
    int PageSize);

public sealed record ExerciseSearchItem(
    string Id,
    string Name,
    string? Code,
    string Category,
    IReadOnlyCollection<string> PrimaryMuscleIds);
