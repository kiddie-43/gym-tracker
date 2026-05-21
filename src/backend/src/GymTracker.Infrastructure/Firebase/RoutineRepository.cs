using GymTracker.Application.Routines;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class RoutineRepository : IRoutineRepository
{
    private const string ModuleName = "routines";
    private readonly SqlDocumentStore _store;

    public RoutineRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task AddAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, routine.UserId, routine.Id, routine, cancellationToken);
    }

    public Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        routine.Touch();
        return _store.UpsertUserAsync(ModuleName, routine.UserId, routine.Id, routine, cancellationToken);
    }

    public Task DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        return _store.DeleteUserAsync(ModuleName, userId, routineId, cancellationToken);
    }

    public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<Routine>(ModuleName, userId, cancellationToken);
    }

    public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<Routine>(ModuleName, userId, routineId, cancellationToken);
    }
}
