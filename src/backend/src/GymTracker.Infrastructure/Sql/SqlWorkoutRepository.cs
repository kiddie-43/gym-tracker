using GymTracker.Application.Workouts;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlWorkoutRepository : IWorkoutRepository
{
    public Task AddAsync(Workout workout, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<Workout>> ListByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<Workout>> ListByUserAndExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Workout?> GetByIdAsync(string userId, string workoutId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpdateAsync(string userId, Workout workout, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(string userId, string workoutId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
