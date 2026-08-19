namespace GymTracker.Domain.Entities;

public sealed class PlanDay
{
    public Guid MonthlyPlanId { get; private set; }
    public int WeekNumber { get; private set; }
    public int DayNumber { get; private set; }
    public PlanDayStatus Status { get; private set; } = PlanDayStatus.Active;
    public DateTimeOffset? TruncatedAt { get; private set; }

    private PlanDay()
    {
    }

    public static PlanDay Create(Guid monthlyPlanId, int weekNumber, int dayNumber)
    {
        if (monthlyPlanId == Guid.Empty) throw new ArgumentException("MonthlyPlanId is required.", nameof(monthlyPlanId));
        if (weekNumber < 1 || weekNumber > 4) throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));
        if (dayNumber < 1 || dayNumber > 7) throw new ArgumentException("DayNumber must be between 1 and 7.", nameof(dayNumber));

        return new PlanDay
        {
            MonthlyPlanId = monthlyPlanId,
            WeekNumber = weekNumber,
            DayNumber = dayNumber,
            Status = PlanDayStatus.Active,
        };
    }

    public void MarkTruncated()
    {
        Status = PlanDayStatus.Truncated;
        TruncatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkActive()
    {
        Status = PlanDayStatus.Active;
        TruncatedAt = null;
    }
}
