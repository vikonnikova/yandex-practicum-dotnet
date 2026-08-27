using Events.Infrastructure.Auth;
using FluentAssertions;

namespace Events.UnitTests.Infrastructure;

public class PasswordHasherTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет вычисление хэша для одного и того же пароля.
    /// </summary>
    [Fact]
    public void Hash_WhenSamePassword_ShouldReturnDifferentHashes()
    {
        //Arrange
        var hasher = new PasswordHasher();

        // Act
        var passwordHash1 = hasher.Hash(UserPassword);
        var passwordHash2 = hasher.Hash(UserPassword);

        // Assert
        passwordHash1.Should().NotBe(passwordHash2);
    }

    /// <summary>
    /// Проверяет, что верный пароль проходит верификацию.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordIsCorrect_ShouldReturnTrue()
    {
        //Arrange
        var hasher = new PasswordHasher();
        var hashedPassword = hasher.Hash(UserPassword);

        //Act
        var result = hasher.Verify(UserPassword, hashedPassword);

        //Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Проверяет, что неверный пароль не проходит верификацию.
    /// </summary>
    [Fact]
    public void Verify_WhenPasswordIsWrong_ShouldReturnFalse()
    {
        //Arrange
        var hasher = new PasswordHasher();
        var hashedPassword = hasher.Hash(UserPassword);

        //Act
        var result = hasher.Verify("qwert1234", hashedPassword);

        //Assert
        result.Should().BeFalse();
    }
}