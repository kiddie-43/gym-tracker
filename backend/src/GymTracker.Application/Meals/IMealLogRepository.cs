using GymTracker.Domain.Entities;

namespace GymTracker.Application.Meals;

public interface IMealLogRepository
{
    Task AddAsync(MealLog mealLog, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MealLog>> ListByUserAndDateAsync(string userId, DateOnly loggedDate, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MealLog>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);
}
