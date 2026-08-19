namespace GymTracker.Domain.Entities;

public sealed class PlanWeek
{
    public Guid MonthlyPlanId { get; private set; }
    public int WeekNumber { get; private set; }

    private readonly List<PlanDay> _days = [];
    public IReadOnlyCollection<PlanDay> Days => _days;

    private PlanWeek()
    {
    }

    public static PlanWeek Create(Guid monthlyPlanId, int weekNumber, IEnumerable<int> dayNumbers)
    {
        if (monthlyPlanId == Guid.Empty) throw new ArgumentException("MonthlyPlanId is required.", nameof(monthlyPlanId));
        if (weekNumber < 1 || weekNumber > 4) throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));

        var week = new PlanWeek
        {
            MonthlyPlanId = monthlyPlanId,
            WeekNumber = weekNumber,
        };

        foreach (var day in dayNumbers.Distinct().OrderBy(x => x))
        {
            week._days.Add(PlanDay.Create(monthlyPlanId, weekNumber, day));
        }

        return week;
    }

    public void EnsureDaysUpTo(int activeDays)
    {
        for (var day = 1; day <= activeDays; day += 1)
        {
            if (_days.All(x => x.DayNumber != day))
            {
                _days.Add(PlanDay.Create(MonthlyPlanId, WeekNumber, day));
            }
        }

        foreach (var day in _days)
        {
            if (day.DayNumber > activeDays)
            {
                day.MarkTruncated();
                continue;
            }

            day.MarkActive();
        }
    }
}
