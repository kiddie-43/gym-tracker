namespace GymTracker.Infrastructure.Firebase;

public static class FirestoreAdminKeyHelper
{
    public static string BuildEntityKey(string collectionName, string documentId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);

        return $"admin/{collectionName}/{documentId}";
    }

    public static string BuildCollectionPrefix(string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        return $"admin/{collectionName}/";
    }

    public static string BuildCodeIndexKey(string collectionName, string normalizedCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedCode);

        return $"admin-index/{collectionName}/code/{normalizedCode}";
    }

    public static string BuildCodeIndexPrefix(string collectionName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(collectionName);

        return $"admin-index/{collectionName}/code/";
    }
}
