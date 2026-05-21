namespace GymTracker.Application.Common;

public static class AuthorizationPolicies
{
    public const string DefaultUserPolicy = "default-user";
    public const string AdminOnlyPolicy = "admin-only";

    public static bool IsUserScopedRequest(string? userId)
    {
        return !string.IsNullOrWhiteSpace(userId) && userId.Length >= 3;
    }

    public static bool IsAdminRequest(bool isAdmin)
    {
        return isAdmin;
    }
}
