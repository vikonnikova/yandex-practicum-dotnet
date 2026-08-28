using Auth.Domain;

namespace Auth.IntegrationTests;

internal static class TestData
{
    public static readonly Guid UserId = Guid.NewGuid();
    public const string Login = "vika_7486";
    public const string Password = "qwerty1234";
    public static User TestUser => User.Create(UserId, Login, Password, UserRole.User);
}