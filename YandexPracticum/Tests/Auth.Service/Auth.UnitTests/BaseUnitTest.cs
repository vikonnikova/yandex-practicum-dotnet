using Auth.Application.Interfaces;
using Auth.Application.UseCases;
using Auth.Domain;
using Auth.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Auth.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
    protected readonly Guid UserId = Guid.NewGuid();
    protected const string UserLogin = "Ivan_123";
    protected const string UserPassword = "qwerty1234";
    protected const int Page = 3;
    protected const int PageSize = 15;

    protected readonly Mock<IJwtProvider> JwtProviderMock = new();
    protected readonly Mock<ICurrentUserContext> UserContextMock = new();
    protected readonly Mock<IPasswordHasher> PasswordHasherMock = new();
    protected readonly Mock<IUserRepository> UserRepositoryMock = new();

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        ConfigureMockServices(services);

        services.AddScoped<RegisterUserCommandHandler>();
        services.AddScoped<LoginCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        var user = User.Create(UserId, UserLogin, "random_string", UserRole.User);

        UserContextMock.Setup(x => x.UserId).Returns(UserId);
        UserContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        JwtProviderMock.Setup(provider => provider.GenerateToken(user))
            .Returns("jwt_token");

        PasswordHasherMock.Setup(hasher => hasher.Hash(UserPassword))
            .Returns("random_password_hash");
        PasswordHasherMock.Setup(hasher => hasher.Verify(UserPassword, It.IsAny<string>()))
            .Returns(true);

        UserRepositoryMock.Setup(repo => repo.FindByLogin(UserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        UserRepositoryMock.Setup(repo => repo.ExistsByLogin(UserLogin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        UserRepositoryMock.Setup(repo => repo.Add(user));
        UserRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        services.AddSingleton(UserContextMock.Object);
        services.AddSingleton(JwtProviderMock.Object);
        services.AddSingleton(PasswordHasherMock.Object);
        services.AddSingleton(UserRepositoryMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}