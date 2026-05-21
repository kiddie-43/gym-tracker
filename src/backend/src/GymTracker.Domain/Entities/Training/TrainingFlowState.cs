namespace GymTracker.Domain.Entities.Training;

public sealed class TrainingFlowState
{
    public string UserId { get; init; } = string.Empty;

    public bool IsLocked { get; set; }

    public string RoutineId { get; set; } = string.Empty;

    public string? SessionId { get; set; }

    public string? ExerciseId { get; set; }

    public string StepNode { get; set; } = "routine";

    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
