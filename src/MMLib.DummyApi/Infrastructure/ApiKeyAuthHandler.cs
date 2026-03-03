using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using MMLib.DummyApi.Configuration;

namespace MMLib.DummyApi.Infrastructure;

/// <summary>
/// Authentication handler that validates requests using an API key header.
/// </summary>
public class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<DummyApiOptions> dummyApiOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private readonly DummyApiOptions _options = dummyApiOptions.Value;

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Api-Key header"));
        }

        var providedApiKey = apiKeyHeaderValues.ToString();

        if (string.IsNullOrWhiteSpace(providedApiKey) || providedApiKey != _options.DefaultApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, "api-user"),
            new Claim(ClaimTypes.Name, "API User")
        ];

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
