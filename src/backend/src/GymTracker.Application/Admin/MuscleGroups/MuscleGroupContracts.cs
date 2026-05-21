namespace GymTracker.Application.Admin.MuscleGroups;

public sealed record UpsertMuscleGroupRequest(
    string Name,
    string Code,
    string? Description,
    bool Active = true);

public sealed record MuscleGroupResponse(
    string Id,
    string Name,
    string Code,
    string? Description,
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt);
