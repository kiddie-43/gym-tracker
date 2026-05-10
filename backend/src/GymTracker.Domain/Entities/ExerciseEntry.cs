namespace GymTracker.Domain.Entities;

public sealed class ExerciseEntry
{
    public string ExternalExerciseId { get; init; } = string.Empty;

    public string ExerciseNameSnapshot { get; init; } = string.Empty;

    public IReadOnlyCollection<string> MuscleGroupIds { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<WorkoutSet> Sets { get; init; } = Array.Empty<WorkoutSet>();

    public string? Notes { get; init; }

    public string? ImageUrl { get; init; }
}

public sealed class WorkoutSet
{
    public int Repetitions { get; init; }

    public decimal? Weight { get; init; }

    public int? RestSeconds { get; init; }

    public bool Completed { get; init; } = true;
}
