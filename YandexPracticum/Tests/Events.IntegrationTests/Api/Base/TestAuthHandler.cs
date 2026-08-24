using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Events.IntegrationTests.Api.Base;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public const string AuthenticationScheme = "TestScheme";

	public TestAuthHandler(
		IOptionsMonitor<AuthenticationSchemeOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder)
		: base(options, logger, encoder)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, TestData.UserId.ToString()),
			new Claim(ClaimTypes.Name, "TestUser"),
			new Claim(ClaimTypes.Role, "Admin")
		};

		var identity = new ClaimsIdentity(claims, AuthenticationScheme);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}