using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Events.Api.Contracts.Users;
using Events.Domain;
using Events.IntegrationTests.Api.Base;
using UserRole = Events.Domain.UserRole;

namespace Events.IntegrationTests.Api;

public class UsersApiTests : BaseApiTest
{
	public UsersApiTests(ApiFixture fixture) : base(fixture)
	{
		Client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
	}

	/// <summary>
	/// Проверяет получение пользователя по идентификатору.
	/// </summary>
	[Fact]
    public async Task GetById_ValidData_200Returned()
	{
		//Arrange
		await Fixture.ExecuteDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(User.Create(TestData.UserId, TestData.Login, TestData.Password, UserRole.User));
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Ivan_585", "Qwerty5678", UserRole.Admin));
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Anton_1187", "567890ytrewq", UserRole.User));
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Kate_433", "12345qwerty", UserRole.User));
			await dbContext.SaveChangesAsync(CancellationToken.None);
		});

		//Act
		var response = await Client.GetAsync($"/users/{TestData.UserId}");

		//Assert
		var responseData = (await response.Content.ReadFromJsonAsync<UserResponse>())!;
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(TestData.UserId, responseData.Id);
		Assert.Equal(TestData.Login, responseData.Login);
		Assert.Equal(Events.Api.Contracts.Users.UserRole.User, responseData.Role);
	}

	/// <summary>
	/// Проверяет получение несуществующего пользователя.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentUser_404Returned()
	{
		//Arrange
		await Fixture.ExecuteDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Ivan_585", "Qwerty5678", UserRole.Admin));
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Anton_1187", "567890ytrewq", UserRole.User));
			dbContext.Users.Add(User.Create(Guid.NewGuid(), "Kate_433", "12345qwerty", UserRole.User));
		});

		//Act
		var response = await Client.GetAsync($"/users/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}