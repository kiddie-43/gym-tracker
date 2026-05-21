using GymTracker.Application.Diets;
using GymTracker.Domain.Entities;
using GymTracker.Infrastructure.Sql;

namespace GymTracker.Infrastructure.Firebase;

public sealed class DietRepository : IDietRepository
{
    private const string ModuleName = "diets";
    private readonly SqlDocumentStore _store;

    public DietRepository(SqlDocumentStore store)
    {
        _store = store;
    }

    public Task AddAsync(Diet diet, CancellationToken cancellationToken = default)
    {
        return _store.UpsertUserAsync(ModuleName, diet.UserId, diet.Id, diet, cancellationToken);
    }

    public Task<IReadOnlyCollection<Diet>> ListByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _store.ListUserAsync<Diet>(ModuleName, userId, cancellationToken);
    }

    public Task<Diet?> GetByUserAndIdAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        return _store.GetUserAsync<Diet>(ModuleName, userId, dietId, cancellationToken);
    }

    public Task UpdateAsync(Diet diet, CancellationToken cancellationToken = default)
    {
        diet.Touch();
        return _store.UpsertUserAsync(ModuleName, diet.UserId, diet.Id, diet, cancellationToken);
    }

    public Task DeleteAsync(string userId, string dietId, CancellationToken cancellationToken = default)
    {
        return _store.DeleteUserAsync(ModuleName, userId, dietId, cancellationToken);
    }
}
