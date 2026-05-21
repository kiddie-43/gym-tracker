namespace GymTracker.Application.Admin.Common;

public static class AdminSoftDeleteFilter
{
    public static IReadOnlyCollection<T> Apply<T>(
        IEnumerable<T> source,
        Func<T, bool> isDeleted,
        Func<T, bool> isActive,
        bool includeInactive = false)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(isDeleted);
        ArgumentNullException.ThrowIfNull(isActive);

        return source
            .Where(item => !isDeleted(item))
            .Where(item => includeInactive || isActive(item))
            .ToArray();
    }
}
