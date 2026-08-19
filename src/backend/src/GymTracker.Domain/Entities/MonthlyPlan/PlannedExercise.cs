using GymTracker.Domain.Common;

namespace GymTracker.Domain.Entities;

public sealed class PlannedExercise : AuditableEntity
{
    public Guid MonthlyPlanId { get; private set; }
    public Guid UserId { get; private set; }
    public int WeekNumber { get; private set; }
    public int DayNumber { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int OrderIndex { get; private set; }

    public static PlannedExercise Create(Guid monthlyPlanId, Guid userId, int weekNumber, int dayNumber, Guid exerciseId, int orderIndex)
    {
        if (monthlyPlanId == Guid.Empty) throw new ArgumentException("MonthlyPlanId is required.", nameof(monthlyPlanId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId is required.", nameof(exerciseId));
        if (weekNumber < 1 || weekNumber > 4) throw new ArgumentException("WeekNumber must be between 1 and 4.", nameof(weekNumber));
        if (dayNumber < 1 || dayNumber > 7) throw new ArgumentException("DayNumber must be between 1 and 7.", nameof(dayNumber));
        if (orderIndex < 0) throw new ArgumentException("OrderIndex must be >= 0.", nameof(orderIndex));

        return new PlannedExercise
        {
            MonthlyPlanId = monthlyPlanId,
            UserId = userId,
            WeekNumber = weekNumber,
            DayNumber = dayNumber,
            ExerciseId = exerciseId,
            OrderIndex = orderIndex,
        };
    }

    public void UpdateExercise(Guid exerciseId)
    {
        if (exerciseId == Guid.Empty) throw new ArgumentException("ExerciseId is required.", nameof(exerciseId));

        ExerciseId = exerciseId;
    }
}
