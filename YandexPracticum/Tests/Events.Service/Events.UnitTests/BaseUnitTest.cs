using Events.Application;
using Events.Application.Interfaces;
using Events.Application.QueryHandlers;
using Events.Application.UseCases;
using Events.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
    protected readonly Guid EventId = Guid.NewGuid();
    protected readonly Guid BookingId = Guid.NewGuid();
    protected readonly Guid UserId = Guid.NewGuid();
    protected const string UserLogin = "Ivan_123";
    protected const string UserPassword = "qwerty1234";
    protected const string EventTitle = "Новый год";
    protected const string EventDescription = "Дед Мороз и снегурочка";
    protected readonly DateTime EventStartAt = new(2022, 01, 01, 00, 00, 00, DateTimeKind.Utc);
    protected readonly DateTime EventEndAt = new(2022, 01, 10, 23, 59, 59, DateTimeKind.Utc);
    protected const int EventTotalSeats = 7;
    protected const int Page = 3;
    protected const int PageSize = 15;

    //protected readonly Mock<IJwtProvider> JwtProviderMock = new();
    //protected readonly Mock<ICurrentUserContext> UserContextMock = new();
    protected readonly Mock<IEventRepository> EventRepositoryMock = new();

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());
        //services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        ConfigureMockServices(services);

        services.AddScoped<GetEventsByQueryHandler>();
        services.AddScoped<GetEventByIdQueryHandler>();

        services.AddScoped<CreateEventCommandHandler>();
        services.AddScoped<UpdateEventCommandHandler>();
        services.AddScoped<DeleteEventCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        //var user = User.Create(UserId, UserLogin, "random_string", UserRole.User);
        var @event = Event.Create(EventId, EventTitle, EventDescription,
            EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);
        //var booking = Booking.Create(BookingId, EventId, UserId, DateTime.UtcNow);

        //UserContextMock.Setup(x => x.UserId).Returns(UserId);
        //UserContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        //JwtProviderMock.Setup(provider => provider.GenerateToken(user))
        //.Returns("jwt_token");

        EventRepositoryMock
            .Setup(repo => repo.GetFiltered(3, 15, new Filters(Title: "День", EventStartAt, EventEndAt),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FilteredResult<Event>(TotalItems: 100, Data: [@event]));
        EventRepositoryMock.Setup(repo => repo.Find(EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);
        EventRepositoryMock.Setup(repo => repo.Add(@event));
        EventRepositoryMock.Setup(repo => repo.Delete(@event));
        EventRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        //services.AddSingleton(UserContextMock.Object);
        //services.AddSingleton(JwtProviderMock.Object);
        services.AddSingleton(EventRepositoryMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}