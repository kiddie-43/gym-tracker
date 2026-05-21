using GymTracker.Domain.Entities.Routines;

namespace GymTracker.Application.Interfaces.Persistence;

public interface IRoutineRepository
{
    Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default);

    Task UpsertAsync(Routine routine, CancellationToken cancellationToken = default);
}
