using FluentAssertions;

using GymTracker.Domain.ValueObjects;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class ExerciseFormFieldEdgeCasesTests
{
    [Fact]
    public void Create_ShouldThrow_WhenSelectFieldHasNoOptions()
    {
        var action = () => ExerciseFormField.Create(
            id: "field-1",
            name: "tempo",
            label: "Tempo",
            type: "select",
            required: true,
            unit: null,
            min: null,
            max: null,
            options: Array.Empty<string>(),
            sortOrder: 0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Select fields require at least one option*");
    }

    [Fact]
    public void Create_ShouldThrow_WhenMinIsGreaterThanMax()
    {
        var action = () => ExerciseFormField.Create(
            id: "field-2",
            name: "repetitions",
            label: "Repeticiones",
            type: "number",
            required: true,
            unit: "reps",
            min: 12,
            max: 6,
            options: null,
            sortOrder: 0);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*min cannot be greater than max*");
    }
}
