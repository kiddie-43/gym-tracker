namespace GymTracker.Api.Options;

public sealed class FirebaseOptions
{
    public const string SectionName = "Firebase";

    public string ProjectId { get; init; } = string.Empty;

    public string? CredentialsPath { get; init; }

    public string? ServiceAccountJson { get; init; }

    public string? StorageBucket { get; init; }

    public string ClientEmail { get; init; } = string.Empty;

    public string PrivateKey { get; init; } = string.Empty;
}
