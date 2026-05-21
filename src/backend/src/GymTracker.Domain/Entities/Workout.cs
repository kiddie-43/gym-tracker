namespace GymTracker.Domain.Entities;

public sealed class Workout : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public DateTimeOffset PerformedAt { get; init; }

    public WorkoutStatus Status { get; init; } = WorkoutStatus.Completed;

    public string? RoutineId { get; init; }

    public string? Notes { get; init; }

    public IReadOnlyCollection<ExerciseEntry> ExerciseEntries { get; init; } = Array.Empty<ExerciseEntry>();
}

public enum WorkoutStatus
{
    Completed = 0,
    Incomplete = 1,
}
