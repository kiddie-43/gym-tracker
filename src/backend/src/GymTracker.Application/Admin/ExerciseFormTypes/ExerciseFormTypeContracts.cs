namespace GymTracker.Application.Admin.ExerciseFormTypes;

public sealed record ExerciseFormFieldRequest(
    string Id,
    string Name,
    string Label,
    string Type,
    bool Required,
    string? Unit,
    decimal? Min,
    decimal? Max,
    IReadOnlyCollection<string>? Options,
    int SortOrder);

public sealed record UpsertExerciseFormTypeRequest(
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<ExerciseFormFieldRequest> Fields,
    bool Active = true);

public sealed record ExerciseFormFieldResponse(
    string Id,
    string Name,
    string Label,
    string Type,
    bool Required,
    string? Unit,
    decimal? Min,
    decimal? Max,
    IReadOnlyCollection<string>? Options,
    int SortOrder);

public sealed record ExerciseFormTypeResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    IReadOnlyCollection<ExerciseFormFieldResponse> Fields,
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);
