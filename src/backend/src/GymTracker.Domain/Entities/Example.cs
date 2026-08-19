namespace GymTracker.Domain.Entities;

public sealed class Example
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public static Example Create(string code, string name, string description)
    {
        return new Example
        {
            Code = NormalizeRequired(code, nameof(code)),
            Name = NormalizeRequired(name, nameof(name)),
            Description = NormalizeRequired(description, nameof(description)),
        };
    }

    public void Update(string code, string name, string description)
    {
        Code = NormalizeRequired(code, nameof(code));
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