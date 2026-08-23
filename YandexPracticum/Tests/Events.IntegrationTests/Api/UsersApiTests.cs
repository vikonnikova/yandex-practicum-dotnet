using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts.Users;
using Events.IntegrationTests.Api.Base;

namespace Events.IntegrationTests.Api;

public class UsersApiTests(ApiFixture fixture) : BaseApiTest(fixture)
{
	#region Get methods

	/// <summary>
	/// Проверяет получение пользователя по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_200Returned()
	{
		//Arrange
		var userId = await CreateUser();

		//Act
		var response = await Client.GetAsync($"/users/{userId}");

		//Assert
		var responseData = (await response.Content.ReadFromJsonAsync<UserResponse>())!;
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(userId, responseData.Id);
		Assert.Equal(TestData.Login, responseData.Login);
		Assert.Equal(TestData.Role, responseData.Role);
	}

	/// <summary>
	/// Проверяет получение несуществующего пользователя.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentUser_404Returned()
	{
		//Arrange
		await CreateUsers();

		//Act
		var response = await Client.GetAsync($"/users/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	#endregion

	#region Create method

	/// <summary>
	/// Проверяет создание пользователя.
	/// </summary>
	[Fact]
	public async Task Create_ValidData_201Returned()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/users", CreateTestUser());

		//Assert
		var userId = await response.Content.ReadFromJsonAsync<Guid>();
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal($"/Users/{userId}", response.Headers.Location!.AbsolutePath);

		var createdUser = (await Client.GetFromJsonAsync<UserResponse>($"/users/{userId}"))!;
		Assert.Equal(userId, createdUser.Id);
		Assert.Equal(TestData.Login, createdUser.Login);
		Assert.Equal(TestData.Role, createdUser.Role);
	}

	/// <summary>
	/// Проверяет создание пользователя с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Create_InvalidData_400Returned()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/users", CreateInvalidTestUser());

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	#endregion
}