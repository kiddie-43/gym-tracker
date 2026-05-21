namespace GymTracker.Domain.Entities;

public sealed class MealLog : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public DateOnly LoggedDate { get; init; }

    public string SlotType { get; init; } = string.Empty;

    public IReadOnlyCollection<MealItem> Items { get; init; } = Array.Empty<MealItem>();
}
