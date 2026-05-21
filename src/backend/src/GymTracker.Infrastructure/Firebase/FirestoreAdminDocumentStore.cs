using GymTracker.Application.Admin.Common;

namespace GymTracker.Infrastructure.Firebase;

public sealed class FirestoreAdminDocumentStore : IAdminDocumentStore
{
    private readonly FirestoreContext _context;

    public FirestoreAdminDocumentStore(FirestoreContext context)
    {
        _context = context;
    }

    public string BuildAdminKey(string collectionName, string documentId)
    {
        return _context.BuildAdminKey(collectionName, documentId);
    }

    public string BuildAdminIndexKey(string collectionName, string normalizedCode)
    {
        return _context.BuildAdminIndexKey(collectionName, normalizedCode);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return _context.GetAsync<T>(key, cancellationToken);
    }

    public Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return _context.SetAsync(key, value, cancellationToken);
    }

    public Task<IReadOnlyCollection<T>> QueryAdminCollectionAsync<T>(string collectionName, CancellationToken cancellationToken = default)
    {
        return _context.QueryAdminCollectionAsync<T>(collectionName, cancellationToken);
    }
}
