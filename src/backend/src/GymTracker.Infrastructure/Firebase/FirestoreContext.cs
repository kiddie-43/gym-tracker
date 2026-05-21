using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GymTracker.Infrastructure.Firebase;

public sealed class FirestoreContext
{
    private const string DocumentsCollectionName = "appDocuments";

    public const string AdminMusclesCollection = "muscles";

    public const string AdminMeasurementTypesCollection = "measurement-types";

    public const string AdminExercisesCollection = "exercises";

    private readonly ConcurrentDictionary<string, object> _inMemoryDocuments = new(StringComparer.OrdinalIgnoreCase);
    private readonly FirestoreDb? _firestoreDb;
    private readonly bool _useInMemory;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public FirestoreContext(IConfiguration configuration, ILogger<FirestoreContext> logger)
    {
        var section = configuration.GetSection("Firebase");
        var projectId = section["ProjectId"];
        var credentialsPath = section["CredentialsPath"];
        var serviceAccountJson = section["ServiceAccountJson"];
        var clientEmail = section["ClientEmail"];
        var privateKey = section["PrivateKey"];

        if (string.IsNullOrWhiteSpace(projectId))
        {
            _useInMemory = true;
            logger.LogWarning("Firebase no configurado: falta Firebase:ProjectId. Se usara almacenamiento en memoria.");
            return;
        }

        try
        {
            // 1) Si ya existe GOOGLE_APPLICATION_CREDENTIALS en el entorno, usar ADC directamente.
            var adcPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (!string.IsNullOrWhiteSpace(adcPath) && File.Exists(adcPath))
            {
                _firestoreDb = FirestoreDb.Create(projectId);
                _useInMemory = false;
                logger.LogInformation("Firestore configurado usando GOOGLE_APPLICATION_CREDENTIALS para el proyecto {ProjectId}.", projectId);
                return;
            }

            // 2) Si se provee ruta en configuración, establecerla y usar ADC.
            if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
                _firestoreDb = FirestoreDb.Create(projectId);
                _useInMemory = false;
                logger.LogInformation("Firestore configurado usando Firebase:CredentialsPath para el proyecto {ProjectId}.", projectId);
                return;
            }

            // 3) Si se provee JSON completo en configuración/secrets, usar JsonCredentials.
            if (!string.IsNullOrWhiteSpace(serviceAccountJson))
            {
                _firestoreDb = CreateFirestoreDbFromJson(projectId, serviceAccountJson);
                _useInMemory = false;
                logger.LogInformation("Firestore configurado usando Firebase:ServiceAccountJson para el proyecto {ProjectId}.", projectId);
                return;
            }
        }
        catch (Exception ex)
        {
            _useInMemory = true;
            logger.LogError(ex, "Error inicializando Firestore con ADC/JSON completo. Se usara almacenamiento en memoria.");
            return;
        }

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(clientEmail) || string.IsNullOrWhiteSpace(privateKey))
        {
            _useInMemory = true;
            logger.LogWarning("Firebase no configurado completo. Se usara almacenamiento en memoria.");
            return;
        }

        try
        {
            var normalizedPrivateKey = privateKey.Replace("\\n", "\n", StringComparison.Ordinal);

            var jsonCredentials = JsonSerializer.Serialize(new
            {
                type = "service_account",
                project_id = projectId,
                client_email = clientEmail,
                private_key = normalizedPrivateKey,
            });

            _firestoreDb = CreateFirestoreDbFromJson(projectId, jsonCredentials);
            _useInMemory = false;
            logger.LogInformation("Firestore configurado usando Firebase:ProjectId/ClientEmail/PrivateKey para el proyecto {ProjectId}.", projectId);
        }
        catch (Exception ex)
        {
            _useInMemory = true;
            logger.LogError(ex, "Error inicializando Firestore. Se usara almacenamiento en memoria.");
        }
    }

    public string BuildUserScopedKey(string userId, string collectionName, string documentId)
    {
        return $"users/{userId}/{collectionName}/{documentId}";
    }

    public string BuildCatalogKey(string collectionName, string documentId)
    {
        return $"catalogCache/{collectionName}/{documentId}";
    }

    public string BuildAdminKey(string collectionName, string documentId)
    {
        return FirestoreAdminKeyHelper.BuildEntityKey(collectionName, documentId);
    }

    public string BuildAdminIndexKey(string collectionName, string normalizedCode)
    {
        return FirestoreAdminKeyHelper.BuildCodeIndexKey(collectionName, normalizedCode);
    }

    public string BuildAdminCollectionPrefix(string collectionName)
    {
        return FirestoreAdminKeyHelper.BuildCollectionPrefix(collectionName);
    }

    public bool IsAdminPrefixCompatible(string prefix)
    {
        return prefix.StartsWith("admin/", StringComparison.OrdinalIgnoreCase)
            || prefix.StartsWith("admin-index/", StringComparison.OrdinalIgnoreCase);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_useInMemory)
        {
            return Task.FromResult(_inMemoryDocuments.TryGetValue(key, out var value) ? (T?)value : default);
        }

        return GetFromFirestoreAsync<T>(key, cancellationToken);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_useInMemory)
        {
            _inMemoryDocuments[key] = value!;
            return;
        }

        var payload = JsonSerializer.Serialize(value, _jsonSerializerOptions);
        var documentRef = GetDocumentsCollection().Document(GetDocumentId(key));
        var document = new Dictionary<string, object>
        {
            ["key"] = key,
            ["payload"] = payload,
            ["updatedAt"] = Timestamp.FromDateTime(DateTime.UtcNow),
        };

        await documentRef.SetAsync(document).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_useInMemory)
        {
            _inMemoryDocuments.TryRemove(key, out _);
            return;
        }

        await GetDocumentsCollection().Document(GetDocumentId(key)).DeleteAsync().ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<T>> QueryByPrefixAsync<T>(string prefix, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_useInMemory)
        {
            var inMemoryResults = _inMemoryDocuments
                .Where(pair => pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && pair.Value is T)
                .Select(pair => (T)pair.Value)
                .ToArray();

            return inMemoryResults;
        }

        var query = GetDocumentsCollection()
            .WhereGreaterThanOrEqualTo("key", prefix)
            .WhereLessThanOrEqualTo("key", prefix + "\uf8ff");

        var snapshot = await query.GetSnapshotAsync().ConfigureAwait(false);
        var results = new List<T>(snapshot.Count);

        foreach (var document in snapshot.Documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!document.TryGetValue<string>("payload", out var payload) || string.IsNullOrWhiteSpace(payload))
            {
                continue;
            }

            var entity = JsonSerializer.Deserialize<T>(payload, _jsonSerializerOptions);
            if (entity is not null)
            {
                results.Add(entity);
            }
        }

        return results;
    }

    public Task<IReadOnlyCollection<T>> QueryAdminCollectionAsync<T>(string collectionName, CancellationToken cancellationToken = default)
    {
        return QueryByPrefixAsync<T>(BuildAdminCollectionPrefix(collectionName), cancellationToken);
    }

    private async Task<T?> GetFromFirestoreAsync<T>(string key, CancellationToken cancellationToken)
    {
        var snapshot = await GetDocumentsCollection().Document(GetDocumentId(key)).GetSnapshotAsync().ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        if (!snapshot.Exists)
        {
            return default;
        }

        if (!snapshot.TryGetValue<string>("payload", out var payload) || string.IsNullOrWhiteSpace(payload))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(payload, _jsonSerializerOptions);
    }

    private CollectionReference GetDocumentsCollection()
    {
        if (_firestoreDb is null)
        {
            throw new InvalidOperationException("FirestoreDb no esta inicializado.");
        }

        return _firestoreDb.Collection(DocumentsCollectionName);
    }

    private static string GetDocumentId(string key)
    {
        return Convert.ToHexString(Encoding.UTF8.GetBytes(key)).ToLowerInvariant();
    }

    private static FirestoreDb CreateFirestoreDbFromJson(string projectId, string jsonCredentials)
    {
        var credential = CredentialFactory.FromJson<ServiceAccountCredential>(jsonCredentials).ToGoogleCredential();

        var builder = new FirestoreDbBuilder
        {
            ProjectId = projectId,
            GoogleCredential = credential,
        };

        return builder.Build();
    }
}
