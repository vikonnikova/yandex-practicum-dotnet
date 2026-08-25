using System.Security.Claims;
using System.Text;
using Events.Application.Interfaces;
using Events.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Events.Infrastructure.Auth;

public class JwtProvider(IConfiguration configuration) : IJwtProvider
{
    public string GenerateToken(User user)
    {
        // 1. Создание списка утверждений (Claims)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Login),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // 2. Ключ подписи из конфига + настройка
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 3. Описание параметров токена через SecurityTokenDescriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        // 4. Генерация токена
        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}