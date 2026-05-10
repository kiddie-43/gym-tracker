using GymTracker.Application.Meals;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class MealLogRepository : UserScopedRepository<MealLog>, IMealLogRepository
{
    public MealLogRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "meal-logs")
    {
    }

    public Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default)
    {
        return SaveAsync(mealLog.UserId, mealLog, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MealLog>> ListByUserAndDateAsync(string userId, DateOnly loggedDate, CancellationToken cancellationToken = default)
    {
        var mealLogs = await ListAsync(userId, cancellationToken);
        return mealLogs.Where(mealLog => mealLog.LoggedDate == loggedDate).ToArray();
    }

    public Task<IReadOnlyCollection<MealLog>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return ListAsync(userId, cancellationToken);
    }
}
