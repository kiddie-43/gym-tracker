namespace GymTracker.Domain.Entities;
using GymTracker.Domain.Common;

public sealed class Units : AuditableEntity
{

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public static Units Create(string code, string name, string description)
    {
        return new Units
        {
            Code = NormalizeRequired(code, nameof(code)),
            Name = NormalizeRequired(name, nameof(name)),
            Description = NormalizeRequired(description, nameof(description)),
        };
    }

    public void Update( string name, string description)
    {
        Name = NormalizeRequired(name, nameof(name));
        Description = NormalizeRequired(description, nameof(description));
    }

    private static string NormalizeRequired(string? value, string paramName)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException($"{paramName} is required.", paramName);
        }

        return normalized;
    }
}