using GymTracker.Domain.Entities;

namespace GymTracker.Application.Workouts;

public interface IWorkoutRepository
{
    Task AddAsync(Workout workout, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Workout>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Workout>> ListByUserAndExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default);

    Task<Workout?> GetByIdAsync(string userId, string workoutId, CancellationToken cancellationToken = default);

    Task UpdateAsync(string userId, Workout workout, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, string workoutId, CancellationToken cancellationToken = default);
}
