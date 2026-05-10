using GymTracker.Application.Meals;
using GymTracker.Application.Workouts;

namespace GymTracker.Infrastructure.Firebase;

public sealed class DashboardSummaryRepository
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMealLogRepository _mealLogRepository;

    public DashboardSummaryRepository(IWorkoutRepository workoutRepository, IMealLogRepository mealLogRepository)
    {
        _workoutRepository = workoutRepository;
        _mealLogRepository = mealLogRepository;
    }

    public async Task<DashboardSummary> GetSummaryAsync(string userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.ListByUserAsync(userId, cancellationToken);
        var meals = await _mealLogRepository.ListByUserAsync(userId, cancellationToken);

        return new DashboardSummary(
            workouts.Count,
            meals.Count,
            workouts.OrderByDescending(w => w.PerformedAt).FirstOrDefault()?.PerformedAt,
            meals.OrderByDescending(m => m.LoggedDate).FirstOrDefault()?.LoggedDate);
    }
}

public sealed record DashboardSummary(int WorkoutCount, int MealLogCount, DateTimeOffset? LastWorkoutAt, DateOnly? LastMealLogDate);
