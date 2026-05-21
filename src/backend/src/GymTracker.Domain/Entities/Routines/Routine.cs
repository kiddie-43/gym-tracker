namespace GymTracker.Domain.Entities.Routines;

public sealed class Routine
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string UserId { get; init; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Goal { get; set; }

    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; private set; }

    public List<RoutineSession> Sessions { get; init; } = new();

    public void Update(string title, string? goal)
    {
        Title = title;
        Goal = goal;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Archive()
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
