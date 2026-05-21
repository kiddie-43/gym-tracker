namespace GymTracker.Application.Admin.ExerciseTypes;

public sealed record UpsertExerciseTypeRequest(
    string Name,
    string Code,
    string? Description,
    string? PrimaryUnit,
    bool RequiresUnits,
    bool Active = true);

public sealed record ExerciseTypeResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    string? PrimaryUnit,
    bool RequiresUnits,
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);
