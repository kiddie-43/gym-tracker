using GymTracker.Application.Routines;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlLegacyRoutineRepository : IRoutineRepository
{
    public Task AddAsync(Routine routine, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
