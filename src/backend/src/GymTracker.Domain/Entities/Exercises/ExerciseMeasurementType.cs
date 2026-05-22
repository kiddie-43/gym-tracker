namespace GymTracker.Domain.Entities;

public sealed class ExerciseMeasurementType
{
    public string ExerciseId { get; init; } = string.Empty;

    public string MeasurementTypeId { get; init; } = string.Empty;

    public int SortOrder { get; init; }
}
