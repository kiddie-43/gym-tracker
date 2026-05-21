namespace GymTracker.Application.Admin.Common;

public static class AdminReactivationGuard
{
    public static void EnsureNoActiveConflict(string code, IEnumerable<string> activeCodes, string parameterName = "code")
    {
        AdminUniqueCodeValidator.EnsureUnique(code, activeCodes, parameterName);
    }
}
