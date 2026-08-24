using Events.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Events.Infrastructure.Auth;

public class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
	public Guid UserId => Guid.Parse(httpContextAccessor.HttpContext?.User
		.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? Guid.Empty.ToString());

	public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}