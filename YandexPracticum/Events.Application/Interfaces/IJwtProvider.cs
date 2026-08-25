using Events.Domain;

namespace Events.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}