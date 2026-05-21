using GymTracker.Application.Workouts;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class WorkoutRepository : IWorkoutRepository
{
    private const string ModuleName = "workouts";
    private readonly SqlDocumentStore _store;

    public WorkoutRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, workout.UserId, workout.Id, workout, cancellationToken);
    }

    public Task<IReadOnlyCollection<Workout>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<Workout>(ModuleName, userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Workout>> ListByUserAndExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var workouts = await _store.ListUserAsync<Workout>(ModuleName, userId, cancellationToken);

        return workouts
            .Where(workout => workout.ExerciseEntries.Any(entry => string.Equals(entry.ExternalExerciseId, exerciseId, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(workout => workout.PerformedAt)
            .ToArray();
    }

    public Task<Workout?> GetByIdAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<Workout>(ModuleName, userId, workoutId, cancellationToken);
    }

    public Task UpdateAsync(string userId, Workout workout, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, userId, workout.Id, workout, cancellationToken);
    }

    public Task DeleteAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        return _store.DeleteUserAsync(ModuleName, userId, workoutId, cancellationToken);
    }

}
