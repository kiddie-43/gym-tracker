namespace GymTracker.Domain.ValueObjects;

public sealed class PlannedExercise
{
    public string ExternalExerciseId { get; init; } = string.Empty;

    public string ExerciseName { get; init; } = string.Empty;


    public int TargetSets { get; init; }

    public int TargetRepetitions { get; init; }

    public int? TargetRestSeconds { get; init; }

    public string? Notes { get; init; }

    public string? ImageUrl { get; init; }
}
