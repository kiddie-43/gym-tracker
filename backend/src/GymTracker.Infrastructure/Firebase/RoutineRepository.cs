using GymTracker.Application.Routines;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public sealed class RoutineRepository : UserScopedRepository<Routine>, IRoutineRepository
{
    public RoutineRepository(FirestoreContext firestoreContext)
        : base(firestoreContext, "routines")
    {
    }

    public Task AddAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        return SaveAsync(routine.UserId, routine, cancellationToken);
    }

    public Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        routine.Touch();
        return SaveAsync(routine.UserId, routine, cancellationToken);
    }

    public new Task DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        return base.DeleteAsync(userId, routineId, cancellationToken);
    }

    public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return ListAsync(userId, cancellationToken);
    }

    public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        return GetAsync(userId, routineId, cancellationToken);
    }
}
