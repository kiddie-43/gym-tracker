
namespace GymTracker.Application.Contracts.Routines;

public sealed record RoutineCardDto(
    string Id,
    string Name,
    string Description,
    DateTimeOffset CreatedAt,
    int ExerciseCount,
    int TotalSessions,
    bool IsDeleted);

public sealed record RoutinesPageResponse(
    IReadOnlyCollection<RoutineCardDto> Items,
    int Total);

public sealed record RoutineDetailDto(
    string Id,
    string Name,
    string Description,
    string? Goal,
    DateTimeOffset CreatedAt,
    bool IsDeleted,
    IReadOnlyCollection<RoutineSessionDto> Sessions,
    int ExerciseCount);

public sealed record RoutineSessionDto(
    string Id,
    string Name,
    IReadOnlyCollection<string> DaysOfWeek,
    int CountExercices);

public sealed record RoutineSessionsPageResponse(
    IReadOnlyCollection<RoutineSessionDto> Items,
    int Total);

public sealed record SessionExerciseDto(
    string Id,
    string ExerciseId,
    string Name);



public sealed record CreateRoutineDto
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    // Backward compatibility for legacy clients that used goal.
    public string? Goal { get; init; }
}

public sealed record UpdateRoutineDto
{
    public string? Name { get; init; }

    public string? Description { get; init; }

    // Backward compatibility for legacy clients that used goal.
    public string? Goal { get; init; }
}

public sealed record CreateRoutineSessionDto(string Name, IReadOnlyCollection<string> DaysOfWeek);

public sealed record UpdateRoutineSessionDto(string Name, IReadOnlyCollection<string> DaysOfWeek);

public sealed record AddSessionExerciseDto(string ExerciseId, string Name);
