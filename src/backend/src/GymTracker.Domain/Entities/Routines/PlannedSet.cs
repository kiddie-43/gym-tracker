namespace GymTracker.Domain.Entities.Routines;

public sealed class PlannedSet
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string SessionExerciseId { get; init; } = string.Empty;

    public int Repetitions { get; private set; }

    public decimal WeightKg { get; private set; }

    public int Order { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public PlannedSet()
    {
    }

    public PlannedSet(string sessionExerciseId, int repetitions, decimal weightKg, int order)
    {
        SessionExerciseId = sessionExerciseId;
        Repetitions = repetitions;
        WeightKg = weightKg;
        Order = order;
    }

    public void Update(int repetitions, decimal weightKg)
    {
        Repetitions = repetitions;
        WeightKg = weightKg;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
