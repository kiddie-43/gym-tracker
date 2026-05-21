using GymTracker.Application.Interfaces.Persistence;
using GymTracker.Domain.Entities.Routines;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase.Repositories;

public sealed class RoutineRepository : IRoutineRepository
{
    private const string ModuleName = "routines-v2";
    private readonly SqlDocumentStore _store;

    public RoutineRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<Routine>(ModuleName, userId, cancellationToken);
    }

    public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<Routine>(ModuleName, userId, routineId, cancellationToken);
    }

    public Task UpsertAsync(Routine routine, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, routine.UserId, routine.Id, routine, cancellationToken);
    }
}
