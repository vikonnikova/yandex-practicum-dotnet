using Auth.Domain;

namespace Auth.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}