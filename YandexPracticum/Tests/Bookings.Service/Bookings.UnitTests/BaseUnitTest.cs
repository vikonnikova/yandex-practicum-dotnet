using Bookings.Application.Interfaces;
using Bookings.Application.QueryHandlers;
using Bookings.Application.UseCases;
using Bookings.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Bookings.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
    protected readonly Guid EventId = Guid.NewGuid();
    protected readonly Guid BookingId = Guid.NewGuid();
    protected readonly Guid UserId = Guid.NewGuid();
    protected const int EventTotalSeats = 7;
    protected const int Page = 3;
    protected const int PageSize = 15;

    //protected readonly Mock<IJwtProvider> JwtProviderMock = new();
    //protected readonly Mock<ICurrentUserContext> UserContextMock = new();
    protected readonly Mock<IBookingRepository> BookingRepositoryMock = new();

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        //services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        ConfigureMockServices(services);

        services.AddScoped<GetBookingByIdQueryHandler>();

        services.AddScoped<CreateBookingCommandHandler>();
        services.AddScoped<CancelBookingCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        //var user = User.Create(UserId, UserLogin, "random_string", UserRole.User);
        var booking = Booking.Create(BookingId, EventId, UserId, DateTime.UtcNow);

        //UserContextMock.Setup(x => x.UserId).Returns(UserId);
        //UserContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        //JwtProviderMock.Setup(provider => provider.GenerateToken(user))
        //.Returns("jwt_token");

        BookingRepositoryMock.Setup(repo => repo.Find(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        BookingRepositoryMock.Setup(repo => repo.GetPending(It.IsAny<CancellationToken>()))
            .ReturnsAsync([Guid.NewGuid(), Guid.NewGuid()]);
        BookingRepositoryMock.Setup(repo => repo.CountPendingByUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        BookingRepositoryMock.Setup(repo => repo.Add(booking));
        BookingRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //services.AddSingleton(UserContextMock.Object);
        //services.AddSingleton(JwtProviderMock.Object);
        services.AddSingleton(BookingRepositoryMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}