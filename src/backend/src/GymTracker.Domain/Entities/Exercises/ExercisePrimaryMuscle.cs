namespace GymTracker.Domain.Entities;

public sealed class ExercisePrimaryMuscle
{
    public string ExerciseId { get; init; } = string.Empty;

    public string MuscleId { get; init; } = string.Empty;

    public int SortOrder { get; init; }
}
