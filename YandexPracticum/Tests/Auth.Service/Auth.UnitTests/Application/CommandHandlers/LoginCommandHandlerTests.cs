using Auth.Application.Contracts.Auth;
using Auth.Application.Exceptions;
using Auth.Application.UseCases;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Auth.UnitTests.Application.CommandHandlers;

public class LoginCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет успешную обработку команды <see cref="LoginCommand"/>>.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<LoginCommandHandler>();

        //Act
        var result = await handler.Handle(new LoginCommand(UserLogin, UserPassword), CancellationToken.None);

        //Assert
        UserRepositoryMock.Verify(
            repo => repo.FindByLogin(It.Is<string>(x => x == UserLogin), It.IsAny<CancellationToken>()),
            Times.Once);

        PasswordHasherMock.Verify(
            hasher => hasher.Verify(It.Is<string>(x => x == UserPassword), It.IsAny<string>()),
            Times.Once);

        result.Should().NotBeNull();
    }

    /// <summary>
    /// Проверяет обработку команды <see cref="LoginCommand"/>> для незарегистрированного в системе пользователя.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUnregisteredUser_ShouldThrowAuthenticationException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<LoginCommandHandler>();
        UserRepositoryMock.Setup(repo => repo.FindByLogin(UserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        //Act
        Func<Task> act = () => handler.Handle(new LoginCommand(UserLogin, UserLogin), CancellationToken.None);
        await act.Should().ThrowAsync<AuthenticationException>();

        //Assert
        UserRepositoryMock.Verify(
            repo => repo.FindByLogin(It.Is<string>(x => x == UserLogin), It.IsAny<CancellationToken>()),
            Times.Once);

        PasswordHasherMock.Verify(
            hasher => hasher.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет обработку команды <see cref="LoginCommand"/>> при неверно введенном пароле.
    /// </summary>
    [Fact]
    public async Task Handle_WhenWrongPassword_ShouldThrowAuthenticationException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<LoginCommandHandler>();
        var command = new LoginCommand(UserLogin, UserPassword);
        PasswordHasherMock.Setup(hasher => hasher.Verify(UserPassword, It.IsAny<string>()))
            .Returns(false);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<AuthenticationException>();

        //Assert
        UserRepositoryMock.Verify(
            repo => repo.FindByLogin(It.Is<string>(x => x == UserLogin), It.IsAny<CancellationToken>()),
            Times.Once);

        PasswordHasherMock.Verify(
            hasher => hasher.Verify(It.Is<string>(x => x == UserPassword), It.IsAny<string>()),
            Times.Once);
    }
}