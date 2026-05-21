using GymTracker.Application.Meals;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class MealLogRepository : IMealLogRepository
{
    private const string ModuleName = "meal-logs";
    private readonly SqlDocumentStore _store;

    public MealLogRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, mealLog.UserId, mealLog.Id, mealLog, cancellationToken);
    }

    public async Task<IReadOnlyCollection<MealLog>> ListByUserAndDateAsync(string userId, DateOnly loggedDate, CancellationToken cancellationToken = default)
    {
        var mealLogs = await _store.ListUserAsync<MealLog>(ModuleName, userId, cancellationToken);
        return mealLogs.Where(mealLog => mealLog.LoggedDate == loggedDate).ToArray();
    }

    public Task<IReadOnlyCollection<MealLog>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<MealLog>(ModuleName, userId, cancellationToken);
    }
}
