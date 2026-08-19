using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities;

public sealed class MonthlyPlan : AuditableEntity
{
    private readonly List<PlanWeek> _weeks = [];
    private readonly List<PlannedExercise> _plannedExercises = [];

    public Guid UserId { get; private set; }
    public int ActiveDays { get; private set; }
    public string MigrationVersion { get; private set; } = "v1";

    public IReadOnlyCollection<PlanWeek> Weeks => _weeks;
    public IReadOnlyCollection<PlannedExercise> PlannedExercises => _plannedExercises;

    public static MonthlyPlan Create(Guid userId, int activeDays)
    {
        ValidateUserId(userId);
        ValidateActiveDays(activeDays);

        var plan = new MonthlyPlan
        {
            UserId = userId,
            ActiveDays = activeDays,
        };

        for (var week = 1; week <= 4; week += 1)
        {
            plan._weeks.Add(PlanWeek.Create(plan.Id, week, Enumerable.Range(1, activeDays)));
        }

        return plan;
    }

    public void UpdateActiveDays(int activeDays)
    {
        ValidateActiveDays(activeDays);
        ActiveDays = activeDays;

        foreach (var week in _weeks)
        {
            week.EnsureDaysUpTo(activeDays);
        }
    }

    public void ReplacePlannedExercises(IEnumerable<PlannedExercise> exercises)
    {
        _plannedExercises.Clear();
        _plannedExercises.AddRange(exercises);
    }

    public void EnsureStructure()
    {
        if (_weeks.Count == 0)
        {
            for (var week = 1; week <= 4; week += 1)
            {
                _weeks.Add(PlanWeek.Create(Id, week, Enumerable.Range(1, ActiveDays)));
            }
        }

        foreach (var week in _weeks)
        {
            week.EnsureDaysUpTo(ActiveDays);
        }
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId is required.", nameof(userId));
        }
    }

    private static void ValidateActiveDays(int activeDays)
    {
        if (activeDays < 1 || activeDays > 7)
        {
            throw new ArgumentException("ActiveDays must be between 1 and 7.", nameof(activeDays));
        }
    }
}
