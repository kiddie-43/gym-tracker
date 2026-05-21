namespace GymTracker.Domain.Entities.Routines;

public sealed class RoutineSession
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string RoutineId { get; init; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<string> DaysOfWeek { get; set; } = new();

    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public List<SessionExerciseLink> Exercises { get; init; } = new();

    public void Update(string name, IEnumerable<string> daysOfWeek)
    {
        Name = name;
        DaysOfWeek = daysOfWeek.Select(day => day.Trim().ToLowerInvariant()).Distinct().ToList();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
