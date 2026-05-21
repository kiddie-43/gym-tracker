using FluentAssertions;

using GymTracker.Domain.Entities;
using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class MeasurementTypeRulesTests
{
    [Fact]
    public void Create_ShouldThrow_WhenNoFieldsProvided()
    {
        var action = () => ExerciseFormType.Create(
            name: "Strength Basic",
            code: "STRENGTH_BASIC",
            description: null,
            fields: Array.Empty<ExerciseFormField>());

        action.Should().Throw<ArgumentException>()
            .WithMessage("*At least one form field is required*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenDuplicateFieldNamesProvided()
    {
        var fields = new[]
        {
            ExerciseFormField.Create("reps-1", "reps", "Reps", "number", true, "reps", 1, 100, null, 1),
            ExerciseFormField.Create("reps-2", "REPS", "Reps duplicated", "number", true, "reps", 1, 100, null, 2),
        };

        var action = () => ExerciseFormType.Create(
            name: "Strength Basic",
            code: "STRENGTH_BASIC",
            description: null,
            fields: fields);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*duplicate names*");
    }
}
