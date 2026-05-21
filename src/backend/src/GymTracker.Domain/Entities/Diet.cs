namespace GymTracker.Domain.Entities;

public sealed class Diet : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public IReadOnlyCollection<DietDay> Days { get; init; } = Array.Empty<DietDay>();
}

public sealed class DietDay
{
    public string DayKey { get; init; } = string.Empty;

    public IReadOnlyCollection<MealSlot> MealSlots { get; init; } = Array.Empty<MealSlot>();
}

public sealed class MealSlot
{
    public string SlotType { get; init; } = string.Empty;

    public IReadOnlyCollection<MealItem> Items { get; init; } = Array.Empty<MealItem>();
}

public sealed class MealItem
{
    public string ExternalFoodId { get; init; } = string.Empty;

    public double Quantity { get; init; }

    public string Unit { get; init; } = string.Empty;

    public double? Calories { get; init; }
}
