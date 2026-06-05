namespace GymTracker.Api.Options;

public sealed class DevelopmentAuthModeOptions
{
    public const string SectionName = "DevelopmentAuthMode";

    public bool Enabled { get; init; }

    public string UserId { get; init; } = "dev-local-user";

    public bool IsAdmin { get; init; } = true;
}