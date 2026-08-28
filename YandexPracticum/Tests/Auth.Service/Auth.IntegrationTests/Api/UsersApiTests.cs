using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Auth.Api.Contracts.Users;
using Auth.Domain;
using Auth.IntegrationTests.Api.Base;
using Events.IntegrationTests.Api.Base;
using UserRole = Auth.Api.Contracts.Users.UserRole;

namespace Auth.IntegrationTests.Api;

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
    public async Task GetById_WhenValidData_ShouldReturn200()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Users.Add(User.Create(TestData.UserId, TestData.Login, TestData.Password, Domain.UserRole.User));
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Ivan_585", "Qwerty5678", Domain.UserRole.Admin));
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Anton_1187", "567890ytrewq", Domain.UserRole.User));
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Kate_433", "12345qwerty", Domain.UserRole.User));
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.GetAsync($"/users/{TestData.UserId}");

        //Assert
        var responseData = (await response.Content.ReadFromJsonAsync<UserResponse>())!;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TestData.UserId, responseData.Id);
        Assert.Equal(TestData.Login, responseData.Login);
        Assert.Equal(UserRole.User, responseData.Role);
    }

    /// <summary>
    /// Проверяет получение несуществующего пользователя.
    /// </summary>
    [Fact]
    public async Task GetById_WhenNonExistentUser_ShouldReturn404()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Ivan_585", "Qwerty5678", Domain.UserRole.Admin));
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Anton_1187", "567890ytrewq", Domain.UserRole.User));
            dbContext.Users.Add(User.Create(Guid.NewGuid(), "Kate_433", "12345qwerty", Domain.UserRole.User));
        });

        //Act
        var response = await Client.GetAsync($"/users/{Guid.NewGuid()}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}