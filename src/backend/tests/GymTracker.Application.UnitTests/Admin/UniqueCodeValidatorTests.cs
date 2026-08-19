using FluentAssertions;

using GymTracker.Application.Admin.Common;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class UniqueCodeValidatorTests
{
    [Fact]
    public void Exists_ShouldMatchCode_IgnoringCaseAndWhitespace()
    {
        var existingCodes = new[] { " strength_basic ", "mobility" };

        var exists = AdminUniqueCodeValidator.Exists("STRENGTH_BASIC", existingCodes);

        exists.Should().BeTrue();
    }

    [Fact]
    public void Exists_ShouldReturnFalse_WhenCodeDoesNotExist()
    {
        var existingCodes = new[] { "strength_basic", "mobility" };

        var exists = AdminUniqueCodeValidator.Exists("conditioning", existingCodes);

        exists.Should().BeFalse();
    }

    [Fact]
    public void EnsureUnique_ShouldThrow_WhenDuplicateExists()
    {
        var action = () => AdminUniqueCodeValidator.EnsureUnique(
            " mobility ",
            new[] { "strength_basic", "MOBILITY" },
            argumentName: "code");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate code*");
    }

    [Fact]
    public void EnsureUnique_ShouldNotThrow_WhenCodeIsUnique()
    {
        var action = () => AdminUniqueCodeValidator.EnsureUnique(
            "conditioning",
            new[] { "strength_basic", "mobility" });

        action.Should().NotThrow();
    }
}
