using FluentAssertions;

using GymTracker.Application.Validation;

namespace GymTracker.Application.UnitTests.Routines;

public sealed class PlannedSetRulesTests
{
    [Fact]
    public void EnsurePlannedSet_ShouldThrow_WhenValuesAreNotPositive()
    {
        var action = () => RoutinesValidationRules.EnsurePlannedSet(0, 0);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EnsurePlannedSet_ShouldPass_WhenValuesArePositive()
    {
        var action = () => RoutinesValidationRules.EnsurePlannedSet(8, 30);

        action.Should().NotThrow();
    }
}
