namespace GymTracker.Domain.Entities.Routines;

public sealed class SessionExerciseLink
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string RoutineId { get; init; } = string.Empty;

    public string SessionId { get; init; } = string.Empty;

    public string ExerciseId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<PlannedSet> PlannedSets { get; init; } = new();

    public void Unlink()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
