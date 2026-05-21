using FluentAssertions;

using GymTracker.Application.Admin.Common;

namespace GymTracker.Application.UnitTests.Admin;

public sealed class SoftDeleteFilterTests
{
    [Fact]
    public void Apply_ShouldExcludeDeletedAndInactive_WhenIncludeInactiveIsFalse()
    {
        var source = new[]
        {
            new AdminItem("1", IsDeleted: false, IsActive: true),
            new AdminItem("2", IsDeleted: false, IsActive: false),
            new AdminItem("3", IsDeleted: true, IsActive: true),
        };

        var filtered = AdminSoftDeleteFilter.Apply(source, item => item.IsDeleted, item => item.IsActive);

        filtered.Select(item => item.Id).Should().BeEquivalentTo(new[] { "1" });
    }

    [Fact]
    public void Apply_ShouldIncludeInactive_WhenIncludeInactiveIsTrue()
    {
        var source = new[]
        {
            new AdminItem("1", IsDeleted: false, IsActive: true),
            new AdminItem("2", IsDeleted: false, IsActive: false),
            new AdminItem("3", IsDeleted: true, IsActive: true),
        };

        var filtered = AdminSoftDeleteFilter.Apply(
            source,
            item => item.IsDeleted,
            item => item.IsActive,
            includeInactive: true);

        filtered.Select(item => item.Id).Should().BeEquivalentTo(new[] { "1", "2" });
    }

    private sealed record AdminItem(string Id, bool IsDeleted, bool IsActive);
}
