using System.Reflection;
using System.Text;
using System.Text.Json;

using FluentAssertions;

namespace GymTracker.Api.IntegrationTests.Admin;

public sealed class AuthorizationTests
{
    [Fact]
    public void ResolveAdminClaim_ShouldReturnTrue_WhenCustomAdminClaimIsTrue()
    {
        var jwt = CreateUnsignedJwt(new Dictionary<string, object>
        {
            ["sub"] = "integration-admin",
            ["custom.admin"] = true,
        });

        ResolveAdminClaim(jwt).Should().BeTrue();
    }

    [Fact]
    public void ResolveAdminClaim_ShouldReturnTrue_WhenNestedCustomAdminClaimIsTrue()
    {
        var jwt = CreateUnsignedJwt(new Dictionary<string, object>
        {
            ["sub"] = "integration-admin",
            ["custom"] = new Dictionary<string, object>
            {
                ["admin"] = true,
            },
        });

        ResolveAdminClaim(jwt).Should().BeTrue();
    }

    [Fact]
    public void ResolveAdminClaim_ShouldReturnFalse_WhenAdminClaimIsMissing()
    {
        var jwt = CreateUnsignedJwt(new Dictionary<string, object>
        {
            ["sub"] = "integration-user",
        });

        ResolveAdminClaim(jwt).Should().BeFalse();
    }

    private static bool ResolveAdminClaim(string jwt)
    {
        var middlewareType = typeof(Program).Assembly.GetType("GymTracker.Api.Middleware.FirebaseAuthMiddleware");
        middlewareType.Should().NotBeNull();

        var method = middlewareType!.GetMethod("ResolveAdminClaim", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var result = method!.Invoke(null, new object[] { jwt });
        result.Should().BeOfType<bool>();
        return (bool)result!;
    }

    private static string CreateUnsignedJwt(Dictionary<string, object> payload)
    {
        var header = new Dictionary<string, object>
        {
            ["alg"] = "none",
            ["typ"] = "JWT",
        };

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);

        return $"{Base64Url(headerJson)}.{Base64Url(payloadJson)}.";
    }

    private static string Base64Url(string value)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
