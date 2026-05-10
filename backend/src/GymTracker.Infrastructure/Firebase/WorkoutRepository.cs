using GymTracker.Application.Workouts;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class WorkoutRepository : UserScopedRepository<Workout>, IWorkoutRepository
{
    public WorkoutRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "workouts")
    {
    }

    public Task AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        return SaveAsync(workout.UserId, workout, cancellationToken);
    }

    public Task<IReadOnlyCollection<Workout>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return ListAsync(userId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Workout>> ListByUserAndExerciseAsync(string userId, string exerciseId, CancellationToken cancellationToken = default)
    {
        var workouts = await ListAsync(userId, cancellationToken);

        return workouts
            .Where(workout => workout.ExerciseEntries.Any(entry => string.Equals(entry.ExternalExerciseId, exerciseId, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(workout => workout.PerformedAt)
            .ToArray();
    }

    public Task<Workout?> GetByIdAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        return GetAsync(userId, workoutId, cancellationToken);
    }

    public Task UpdateAsync(string userId, Workout workout, CancellationToken cancellationToken = default)
    {
        return SaveAsync(userId, workout, cancellationToken);
    }

    public new Task DeleteAsync(string userId, string workoutId, CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync(userId, workoutId, cancellationToken);
    }

}
