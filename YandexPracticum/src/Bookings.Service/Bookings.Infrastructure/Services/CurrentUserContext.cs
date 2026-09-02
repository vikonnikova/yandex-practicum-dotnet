using Bookings.Application.Exceptions;
using Bookings.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Bookings.Infrastructure.Services;

internal class CurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public Guid UserId
    {
        get
        {
            var subClaimValue = httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return string.IsNullOrEmpty(subClaimValue)
                ? throw new AuthenticationException()
                : Guid.Parse(subClaimValue);
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false; //TODO
}