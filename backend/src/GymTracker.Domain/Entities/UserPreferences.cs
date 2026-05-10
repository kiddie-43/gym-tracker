namespace GymTracker.Domain.Entities;

public sealed class UserPreferences : BaseEntity
{
    public string UserId { get; init; } = string.Empty;

    public bool CalorieTrackingEnabled { get; init; }

    public int? DailyCalorieGoal { get; init; }
}
