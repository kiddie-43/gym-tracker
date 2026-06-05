namespace GymTracker.Domain.Entities;

using System.Collections.ObjectModel;
using GymTracker.Domain.Common;

public sealed class Exercice : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    public string Code { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Collection<ExerciceUnit> Units { get; private set; } = new();

    public Collection<ExerciceMuscle> Muscles { get; private set; } = new();

    private Exercice()
    {
    }

    public static Exercice Create(string name, string code, string? description)
    {
        return new Exercice
        {
            Name = ValidateName(name),
            Code = NormalizeCode(code),
            Description = NormalizeDescription(description),
        };
    }

    public void Update(string name, string? description)
    {
        Name = ValidateName(name);
        Description = NormalizeDescription(description);
    }

    public void AddUnit(Units unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        if (Units.Any(x => x.UnitId == unit.Id))
        {
            return;
        }

        Units.Add(new ExerciceUnit(this, unit));
    }

    public void AddPrimaryMuscle(Muscle muscle)
    {
        AddMuscle(muscle, ExerciceMuscleType.Primary);
    }

    public void AddSecondaryMuscle(Muscle muscle)
    {
        AddMuscle(muscle, ExerciceMuscleType.Secondary);
    }

    private void AddMuscle(Muscle muscle, ExerciceMuscleType type)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        if (Muscles.Any(x => x.MuscleId == muscle.Id && x.Type == type))
        {
            return;
        }

        Muscles.Add(new ExerciceMuscle(this, muscle, type));
    }

    public void ClearUnits()
    {
        Units.Clear();
    }

    public void ClearMuscles()
    {
        Muscles.Clear();
    }

    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Name is required.", nameof(value));
        }

        return value.Trim();
    }

    private static string NormalizeCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Code is required.", nameof(value));
        }

        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeDescription(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}