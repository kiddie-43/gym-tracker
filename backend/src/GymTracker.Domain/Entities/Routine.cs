using GymTracker.Domain.ValueObjects;

namespace GymTracker.Domain.Entities;

public sealed class Routine : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public IReadOnlyCollection<string> Tags { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<RoutineDay> Days { get; init; } = Array.Empty<RoutineDay>();
}

public sealed class RoutineDay
{
    public string DayLabel { get; init; } = string.Empty;

    public IReadOnlyCollection<PlannedExercise> Exercises { get; init; } = Array.Empty<PlannedExercise>();
}
