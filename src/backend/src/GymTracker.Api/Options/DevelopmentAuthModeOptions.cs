namespace GymTracker.Api.Options;

public sealed class DevelopmentAuthModeOptions
{
    public const string SectionName = "DevelopmentAuthMode";

    public bool Enabled { get; init; }

    public string UserId { get; init; } = "11111111-1111-1111-1111-111111111111";

    public bool IsAdmin { get; init; } = true;
}