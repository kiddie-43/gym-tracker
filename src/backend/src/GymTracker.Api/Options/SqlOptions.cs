namespace GymTracker.Api.Options;

public sealed class SqlOptions
{
    public const string SectionName = "Sql";

    public bool Enabled { get; init; }

    public string? ConnectionString { get; init; }
}
