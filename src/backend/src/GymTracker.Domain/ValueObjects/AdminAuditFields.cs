namespace GymTracker.Domain.ValueObjects;

public sealed record AdminAuditFields(
    bool Active,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? DeletedAt)
{
    public static AdminAuditFields CreateActiveNow(DateTimeOffset now)
        => new(Active: true, IsDeleted: false, CreatedAt: now, UpdatedAt: now, DeletedAt: null);
}
