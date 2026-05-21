using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.Admin.Exercises;

public sealed record UpsertExerciseRequest(
    string Name,
    string Code,
    string? Description,
    string Category,
    string Difficulty,
    string MeasurementTypeId,
    string MeasurementTypeCode,
    IReadOnlyCollection<string> PrimaryMuscleIds,
    IReadOnlyCollection<string> SecondaryMuscleIds,
    IReadOnlyCollection<string> MuscleGroupIds,
    bool Active = true)
{
    public IReadOnlyCollection<string> MeasurementTypeIds => string.IsNullOrWhiteSpace(MeasurementTypeId)
        ? Array.Empty<string>()
        : new[] { MeasurementTypeId };

    public string ExerciseTypeId => Category;

    public string ExerciseTypeCode => Difficulty;

    public string FormTypeId => MeasurementTypeId;

    public string FormTypeCode => MeasurementTypeCode;
}

public sealed record ExerciseResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    string Category,
    string Difficulty,
    string MeasurementTypeId,
    string MeasurementTypeCode,
    IReadOnlyCollection<string> PrimaryMuscleIds,
    IReadOnlyCollection<string> SecondaryMuscleIds,
    IReadOnlyCollection<string> MuscleGroupIds,
    string? CoverStoragePath,
    bool Active,
    bool IsDeleted,
    IReadOnlyCollection<ExerciseMedia> Media,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt)
{
    public string ExerciseTypeId => Category;

    public string ExerciseTypeCode => Difficulty;

    public string FormTypeId => MeasurementTypeId;

    public string FormTypeCode => MeasurementTypeCode;
}

public sealed record ExercisesPageResponse(
    IReadOnlyCollection<ExerciseResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record WorkoutCatalogExerciseDto(
    string Id,
    string Name,
    IReadOnlyCollection<string> MuscleGroupIds,
    string? CoverStoragePath,
    string MeasurementTypeId,
    string MeasurementTypeCode)
{
    public string FormTypeId => MeasurementTypeId;

    public string FormTypeCode => MeasurementTypeCode;
}

public sealed record ImportExerciseRowRequest(
    string? Code,
    string? Name,
    string? Description,
    string? Category,
    string? Difficulty,
    string? MeasurementTypeId,
    string? MeasurementTypeCode,
    IReadOnlyCollection<string>? PrimaryMuscleIds,
    IReadOnlyCollection<string>? SecondaryMuscleIds);

public sealed record ImportExercisesRequest(
    IReadOnlyCollection<ImportExerciseRowRequest>? Rows);

public sealed record ImportExerciseRowResult(
    int RowNumber,
    string? Code,
    bool Created,
    string? Reason,
    ExerciseResponse? Exercise);

public sealed record ImportExercisesResult(
    int TotalRows,
    int CreatedRows,
    int RejectedRows,
    IReadOnlyCollection<ImportExerciseRowResult> Rows);
