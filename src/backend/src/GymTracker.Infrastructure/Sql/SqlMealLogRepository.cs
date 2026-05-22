using GymTracker.Application.Meals;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlMealLogRepository : IMealLogRepository
{
    public Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<MealLog>> ListByUserAndDateAsync(string userId, DateOnly loggedDate, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<MealLog>> ListByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
