using GymTracker.Application.Diets;
using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Sql;

public sealed class SqlDietRepository : IDietRepository
{
    public Task AddAsync(Diet diet, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyCollection<Diet>> ListByUserAsync(string userId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<Diet?> GetByUserAndIdAsync(string userId, string dietId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task UpdateAsync(Diet diet, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
