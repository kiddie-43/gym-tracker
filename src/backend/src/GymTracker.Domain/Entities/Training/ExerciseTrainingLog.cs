namespace GymTracker.Domain.Entities.Training;

public sealed class ExerciseTrainingLog
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string UserId { get; init; } = string.Empty;

    public string RoutineId { get; init; } = string.Empty;

    public string SessionId { get; init; } = string.Empty;

    public string ExerciseId { get; init; } = string.Empty;

    public List<PerformedSet> PerformedSets { get; init; } = new();

    public string? Notes { get; init; }

    public List<TrainingAttachment> Attachments { get; init; } = new();

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; init; } = DateTimeOffset.UtcNow;
}

public sealed class PerformedSet
{
    public int Repetitions { get; init; }

    public decimal WeightKg { get; init; }

    public int Order { get; init; }
}
