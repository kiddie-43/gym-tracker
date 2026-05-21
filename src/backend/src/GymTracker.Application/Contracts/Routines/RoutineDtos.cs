namespace GymTracker.Application.Contracts.Routines;

public sealed record RoutineCardDto(string Id, string Title, DateTimeOffset CreatedAt, int ExerciseCount, bool IsDeleted);

public sealed record RoutineDetailDto(
    string Id,
    string Title,
    string? Goal,
    DateTimeOffset CreatedAt,
    bool IsDeleted,
    IReadOnlyCollection<RoutineSessionDto> Sessions,
    int ExerciseCount);

public sealed record RoutineSessionDto(
    string Id,
    string Name,
    IReadOnlyCollection<string> DaysOfWeek,
    IReadOnlyCollection<SessionExerciseDto> Exercises);

public sealed record SessionExerciseDto(
    string Id,
    string ExerciseId,
    string Name,
    IReadOnlyCollection<PlannedSetDto> PlannedSets);

public sealed record PlannedSetDto(string Id, int Repetitions, decimal WeightKg, int Order);

public sealed record CreateRoutineDto(string Title, string? Goal);

public sealed record UpdateRoutineDto(string? Title, string? Goal);

public sealed record CreateRoutineSessionDto(string Name, IReadOnlyCollection<string> DaysOfWeek);

public sealed record AddSessionExerciseDto(string ExerciseId, string Name);

public sealed record UpdatePlannedSetDto(int Repetitions, decimal WeightKg);
