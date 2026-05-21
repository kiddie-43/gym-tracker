using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using GymTracker.Application.Admin.Exercises;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GymTracker.Infrastructure.Storage;

public sealed class StorageService : IExerciseStorageService
{
    private readonly StorageClient? _storageClient;
    private readonly UrlSigner? _urlSigner;
    private readonly string _bucketName;
    private readonly ILogger<StorageService> _logger;
    private readonly IHostEnvironment _hostEnvironment;

    public StorageService(IConfiguration configuration, ILogger<StorageService> logger, IHostEnvironment hostEnvironment)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;

        var section = configuration.GetSection("Firebase");
        _bucketName = section["StorageBucket"] ?? string.Empty;
        var serviceAccountJson = section["ServiceAccountJson"];
        var credentialsPath = section["CredentialsPath"];

        if (string.IsNullOrWhiteSpace(_bucketName) && _hostEnvironment.IsProduction())
        {
            throw new InvalidOperationException("Firebase:StorageBucket is required to initialize StorageService.");
        }

        if (string.IsNullOrWhiteSpace(_bucketName))
        {
            _bucketName = "dev-bucket";
            _logger.LogWarning("Firebase:StorageBucket is missing. Using development fallback bucket name.");
        }

        GoogleCredential? credential = null;
        ServiceAccountCredential? serviceAccountCredential = null;
        if (!string.IsNullOrWhiteSpace(serviceAccountJson))
        {
            serviceAccountCredential = CredentialFactory.FromJson<ServiceAccountCredential>(serviceAccountJson);
            credential = serviceAccountCredential.ToGoogleCredential();
        }
        else if (!string.IsNullOrWhiteSpace(credentialsPath) && File.Exists(credentialsPath))
        {
            serviceAccountCredential = CredentialFactory.FromFile<ServiceAccountCredential>(credentialsPath);
            credential = serviceAccountCredential.ToGoogleCredential();
        }
        else
        {
            if (_hostEnvironment.IsProduction())
            {
                credential = GoogleCredential.GetApplicationDefault();
            }
            else
            {
                credential = null;
            }
        }

        _storageClient = credential is null ? null : StorageClient.Create(credential);

        if (serviceAccountCredential is not null)
        {
            _urlSigner = UrlSigner.FromCredential(serviceAccountCredential);
        }
        else if (credential?.UnderlyingCredential is ServiceAccountCredential serviceAccount)
        {
            _urlSigner = UrlSigner.FromCredential(serviceAccount);
        }
        else if (_hostEnvironment.IsProduction())
        {
            throw new InvalidOperationException("Service account credentials are required to generate signed URLs.");
        }
        else
        {
            _urlSigner = null;
            _logger.LogWarning("StorageService initialized without signing credentials. Falling back to development signed URL simulation.");
        }
    }

    public async Task<string> GenerateUploadUrlAsync(
        string storagePath,
        TimeSpan expiresIn,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new ArgumentException("Storage path is required.", nameof(storagePath));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException("Content type is required.", nameof(contentType));
        }

        if (_urlSigner is null)
        {
            return $"https://storage.googleapis.com/{_bucketName}/{storagePath}?dev_signed=true";
        }

        var signedUrl = await _urlSigner.SignAsync(_bucketName, storagePath, expiresIn, HttpMethod.Put, signingVersion: null, cancellationToken).ConfigureAwait(false);
        return signedUrl;
    }

    public async Task<bool> ExistsAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_hostEnvironment.IsProduction())
        {
            return true;
        }

        if (_storageClient is null)
        {
            throw new InvalidOperationException("Storage client is not initialized.");
        }

        try
        {
            var obj = await _storageClient.GetObjectAsync(_bucketName, storagePath, cancellationToken: cancellationToken).ConfigureAwait(false);
            return obj is not null;
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_hostEnvironment.IsProduction())
        {
            return;
        }

        if (_storageClient is null)
        {
            throw new InvalidOperationException("Storage client is not initialized.");
        }

        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, storagePath, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Storage object already deleted: {StoragePath}", storagePath);
        }
    }
}
