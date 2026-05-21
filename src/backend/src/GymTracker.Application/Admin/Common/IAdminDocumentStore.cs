namespace GymTracker.Application.Admin.Common;

public interface IAdminDocumentStore
{
    string BuildAdminKey(string collectionName, string documentId);

    string BuildAdminIndexKey(string collectionName, string normalizedCode);

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<T>> QueryAdminCollectionAsync<T>(string collectionName, CancellationToken cancellationToken = default);
}
