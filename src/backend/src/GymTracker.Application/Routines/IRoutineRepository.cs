using GymTracker.Domain.Entities;

namespace GymTracker.Application.Routines;

public interface IRoutineRepository
{
    Task AddAsync(Routine routine, CancellationToken cancellationToken = default);

    Task UpdateAsync(Routine routine, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, string routineId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Routine>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<Routine?> GetByUserAndIdAsync(string userId, string routineId, CancellationToken cancellationToken = default);
}
