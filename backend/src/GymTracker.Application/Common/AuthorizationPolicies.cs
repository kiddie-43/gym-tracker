namespace GymTracker.Application.Common;

public static class AuthorizationPolicies
{
    public const string DefaultUserPolicy = "default-user";

    public static bool IsUserScopedRequest(string? userId)
    {
        return !string.IsNullOrWhiteSpace(userId) && userId.Length >= 3;
    }
}
