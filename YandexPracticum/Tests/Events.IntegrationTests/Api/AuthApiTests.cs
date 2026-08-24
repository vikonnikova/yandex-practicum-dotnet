using System.Net;
using System.Net.Http.Json;
using Events.Domain;
using Events.IntegrationTests.Api.Base;

namespace Events.IntegrationTests.Api;

public class AuthApiTests(ApiFixture fixture) : BaseApiTest(fixture)
{
	/// <summary>
	/// Проверяет успешную регистрацию пользователя в системе.
	/// </summary>
	[Fact]
	public async Task Register_WhenValidData_ShouldReturn204()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/auth/register",
			new { Login = TestData.Login, Password = TestData.Password, Role = UserRole.User });

		//Assert
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
	}

	/// <summary>
	/// Проверяет неуспешную регистрацию пользователя в системе.
	/// </summary>
	[Fact]
	public async Task Register_WhenInvalidData_ShouldReturn400()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/auth/register",
			new { Login = TestData.Login, Role = UserRole.User });

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	/// <summary>
	/// Проверяет регистрацию пользователя с уже зарегистрированным логином.
	/// </summary>
	[Fact]
	public async Task Register_WhenUserExists_ShouldReturn409()
	{
		//Arrange
		await CreateUser();

		//Act
		var response = await Client.PostAsJsonAsync("/auth/register",
			new { Login = TestData.Login, Password = TestData.Password, Role = UserRole.User });

		//Assert
		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
	}

	/// <summary>
	/// Проверяет успешный вход пользователя в систему.
	/// </summary>
	[Fact]
	public async Task Login_WhenValidData_ShouldReturn200()
	{
		//Arrange
		await CreateUser();

		//Act
		var response = await Client.PostAsJsonAsync("/auth/login",
			new { Login = TestData.Login, Password = TestData.Password });

		//Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}

	/// <summary>
	/// Проверяет неуспешный вход пользователя в систему.
	/// </summary>
	[Fact]
	public async Task Login_WhenInvalidData_ShouldReturn400()
	{
		//Arrange
		await CreateUser();

		//Act
		var response = await Client.PostAsJsonAsync("/auth/login", new { Password = TestData.Password });

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	/// <summary>
	/// Проверяет вход в систему незарегистрированного пользователя.
	/// </summary>
	[Fact]
	public async Task Login_WhenUnregisteredUser_ShouldReturn401()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/auth/login",
			new { Login = TestData.Login, Password = TestData.Password });

		//Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	/// <summary>
	/// Проверяет вход в систему пользователя с неверным логином.
	/// </summary>
	[Fact]
	public async Task Login_WhenWrongLogin_ShouldReturn401()
	{
		//Arrange
		await CreateUser();

		//Act
		var response = await Client.PostAsJsonAsync("/auth/login",
			new { Login = "vika7486", Password = TestData.Password });

		//Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	/// <summary>
	/// Проверяет вход в систему пользователя с неверным паролем.
	/// </summary>
	[Fact]
	public async Task Login_WhenWrongPassword_ShouldReturn401()
	{
		//Arrange
		await CreateUser();
		
		//Act
		var response = await Client.PostAsJsonAsync("/auth/login",
			new { Login = TestData.Login, Password = "qwerty123" });

		//Assert
		Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
	}

	private async Task CreateUser()
	{
		await Fixture.ExecuteDbContextAsync(async dbContext => 
		{
			dbContext.Users.Add(User.Create(Guid.NewGuid(), TestData.Login, TestData.PasswordHash, UserRole.User));
		});
	}
}