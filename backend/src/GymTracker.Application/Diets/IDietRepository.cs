using GymTracker.Domain.Entities;

namespace GymTracker.Application.Diets;

public interface IDietRepository
{
    Task AddAsync(Diet diet, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Diet>> ListByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<Diet?> GetByUserAndIdAsync(string userId, string dietId, CancellationToken cancellationToken = default);

    Task UpdateAsync(Diet diet, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default);
}
