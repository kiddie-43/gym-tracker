using GymTracker.Domain.Entities;

namespace GymTracker.Infrastructure.Firebase;

public abstract class UserScopedRepository<T> where T : BaseEntity
{
    private readonly FirestoreContext _firestoreContext;
    private readonly string _collectionName;

    protected UserScopedRepository(FirestoreContext firestoreContext, string collectionName)
    {
        _firestoreContext = firestoreContext;
        _collectionName = collectionName;
    }

    protected Task<T?> GetAsync(string userId, string id, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.GetAsync<T>(_firestoreContext.BuildUserScopedKey(userId, _collectionName, id), cancellationToken);
    }

    protected Task SaveAsync(string userId, T entity, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.SetAsync(_firestoreContext.BuildUserScopedKey(userId, _collectionName, entity.Id), entity, cancellationToken);
    }

    protected Task DeleteAsync(string userId, string id, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.DeleteAsync(_firestoreContext.BuildUserScopedKey(userId, _collectionName, id), cancellationToken);
    }

    protected Task<IReadOnlyCollection<T>> ListAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _firestoreContext.QueryByPrefixAsync<T>($"users/{userId}/{_collectionName}/", cancellationToken);
    }
}
