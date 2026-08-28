using Auth.Domain;
using FluentAssertions;

namespace Auth.UnitTests.Domain;

public class UserTests
{
    /// <summary>
    /// Проверяет создание пользователя.
    /// </summary>
    [Fact]
    public void Create_WhenValidData_ShouldWorkCorrectly()
    {
        var userId = Guid.NewGuid();
        const string login = "vika_7486";
        const string passwordHash = "qwerty1234";
        const UserRole role = UserRole.Admin;

        var user = User.Create(userId, login, passwordHash, role);

        user.Id.Should().Be(userId);
        user.Login.Should().Be(login);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
    }
}