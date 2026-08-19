using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GymTracker.Api.Middleware;

public static class AuthSchemes
{
    public const string FirebaseBearer = "FirebaseBearer";
}

// This handler allows authorization middleware to return proper 401/403 responses.
// Actual principal resolution is performed by FirebaseAuthMiddleware.
public sealed class PassthroughAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public PassthroughAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}
