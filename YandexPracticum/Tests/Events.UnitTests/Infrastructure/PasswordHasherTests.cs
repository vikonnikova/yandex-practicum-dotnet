using Events.Infrastructure.Auth;
using FluentAssertions;

namespace Events.UnitTests.Infrastructure;

public class PasswordHasherTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет успешное вычисление хэша.
	/// </summary>
	[Fact]
	public void Hash_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		var hasher = new PasswordHasher();

		//Act
		var passwordHash = hasher.Hash(UserPassword);

		//Assert
		passwordHash.Should().Be(UserPasswordHash);
	}

	/// <summary>
	/// Проверяет, что верный пароль проходит верификацию.
	/// </summary>
	[Fact]
	public void Verify_WhenPasswordIsCorrect_ShouldReturnTrue()
	{
		//Arrange
		var hasher = new PasswordHasher();

		//Act
		var result = hasher.Verify(UserPassword, UserPasswordHash);

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

		//Act
		var result = hasher.Verify("qwert1234", UserPasswordHash);

		//Assert
		result.Should().BeFalse();
	}
}