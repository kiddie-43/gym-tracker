namespace GymTracker.Infrastructure.Wger;

public sealed class WgerOptions
{
    public const string SectionName = "Wger";

    public string BaseUrl { get; init; } = "https://wger.de/api/v2/";

    public string ApiKey { get; init; } = string.Empty;

    public string LanguageCode { get; init; } = "es";
}