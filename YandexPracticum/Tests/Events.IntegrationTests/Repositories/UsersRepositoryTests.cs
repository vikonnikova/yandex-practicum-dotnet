using Events.Domain;
using Events.Infrastructure;
using Events.IntegrationTests.Repositories.Base;
using FluentAssertions;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class UsersRepositoryTests(DbFixture dbFixture) : BaseRepositoryTest(dbFixture)
{
	/// <summary>
	/// Проверяет поиск пользователя по логину.
	/// </summary>
	[Fact]
	public async Task FindByLogin_WhenValidData_ShouldReturnBooking()
	{
		// Arrange
		const string hash = "b512c1b48074900a6316ea570f7cf738";
		var user = User.Create(UserId, TestData.Login, hash, UserRole.Admin);

		await using (var context = DbFixture.CreateContext())
		{
			context.Users.Add(user);
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var result = await new UserRepository(context).FindByLogin(TestData.Login, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Id.Should().Be(UserId);
			result.Login.Should().Be(TestData.Login);
			result.PasswordHash.Should().Be(hash);
			result.Role.Should().Be(UserRole.Admin);
		}
	}

	/// <summary>
	/// Проверяет поиск несуществующего пользователя по логину.
	/// </summary>
	[Fact]
	public async Task FindByLogin_WhenUserDoesNotExist_ShouldReturnNull()
	{
		// Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var result = await new UserRepository(context).FindByLogin("vika7486", CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Проверяет поиск пользователя по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenValidData_ShouldReturnUser()
	{
		// Arrange
		const string hash = "b512c1b48074900a6316ea570f7cf738";
		var user = User.Create(UserId, TestData.Login, hash, UserRole.Admin);

		await using (var context = DbFixture.CreateContext())
		{
			context.Users.Add(user);
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var result = await new UserRepository(context).Find(UserId, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Id.Should().Be(UserId);
			result.Login.Should().Be(TestData.Login);
			result.PasswordHash.Should().Be(hash);
			result.Role.Should().Be(UserRole.Admin);
		}
	}

	/// <summary>
	/// Проверяет поиск несуществующего пользователя по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenUserDoesNotExist_ShouldReturnNull()
	{
		// Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var result = await new UserRepository(context).Find(Guid.NewGuid(), CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Проверяет Проверяет поиск зарегистрированного пользователя по логину.
	/// </summary>
	[Fact]
	public async Task ExistsByLogin_WhenUserExists_ShouldReturnTrue()
	{
		// Arrange
		const string hash = "b512c1b48074900a6316ea570f7cf738";
		var user = User.Create(UserId, TestData.Login, hash, UserRole.Admin);

		await using (var context = DbFixture.CreateContext())
		{
			context.Users.Add(user);
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var repository = new UserRepository(context);
			var result = await repository.ExistsByLogin(TestData.Login, CancellationToken.None);

			// Assert
			result.Should().BeTrue();
		}
	}

	/// <summary>
	/// Проверяет поиск незарегистрированного пользователя по логину.
	/// </summary>
	[Fact]
	public async Task ExistsByLogin_WhenUserDoesNotExist_ShouldReturnFalse()
	{
		// Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var repository = new UserRepository(context);
		var result = await repository.ExistsByLogin(TestData.Login, CancellationToken.None);

		// Assert
		result.Should().BeFalse();
	}

	/// <summary>
	/// Проверяет сохранение пользователя (через цепочку Add + SaveChangesAsync).
	/// </summary>
	[Fact]
	public async Task Add_WhenValidData_ShouldSaveCorrectly()
	{
		// Arrange
		const string hash = "b512c1b48074900a6316ea570f7cf738";

		// Act
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new UserRepository(context);
			repository.Add(User.Create(UserId, TestData.Login, hash, UserRole.User));
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Assert
		await using (var context = DbFixture.CreateContext())
		{
			var result = await context.Users.FindAsync(UserId);

			result.Should().NotBeNull();
			result.Id.Should().Be(UserId);
			result.Login.Should().Be(TestData.Login);
			result.PasswordHash.Should().Be(hash);
			result.Role.Should().Be(UserRole.User);
		}
	}

	/// <summary>
	/// Проверяет сохранение пользователя с уже существующим логином (через цепочку Add + SaveChangesAsync).
	/// </summary>
	[Fact]
	public async Task Add_WhenUserWithTheSameLoginAlreadyExists_ShouldTrowUniqueException()
	{
		// Arrange
		const string hash = "b512c1b48074900a6316ea570f7cf738";
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new UserRepository(context);
			repository.Add(User.Create(UserId, TestData.Login, hash, UserRole.User));
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Act
		await using (var context = DbFixture.CreateContext())
		{
			var repository = new UserRepository(context);
			repository.Add(User.Create(UserId, TestData.Login, hash, UserRole.User));

			//Assert
			Func<Task> act = async () => await repository.SaveChangesAsync(CancellationToken.None);
			await act.Should().ThrowAsync<Microsoft.EntityFrameworkCore.DbUpdateException>();
		}
	}
}