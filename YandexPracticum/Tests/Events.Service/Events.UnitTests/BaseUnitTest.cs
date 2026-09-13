using Events.Application;
using Events.Application.Interfaces;
using Events.Application.QueryHandlers;
using Events.Application.UseCases;
using Events.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shared.Contracts;
using Shared.Settings;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
    protected readonly Guid UserId = Guid.NewGuid();
    protected readonly Guid EventId = Guid.NewGuid();
    protected const string EventTitle = "Новый год";
    protected const string EventDescription = "Дед Мороз и снегурочка";
    protected readonly DateTime EventStartAt = new(2022, 01, 01, 00, 00, 00, DateTimeKind.Utc);
    protected readonly DateTime EventEndAt = new(2022, 01, 10, 23, 59, 59, DateTimeKind.Utc);
    protected const int EventTotalSeats = 7;
    protected const int Page = 3;
    protected const int PageSize = 15;

    protected readonly Mock<IEventRepository> EventRepositoryMock = new();
    protected readonly Mock<ICurrentUserContext> UserContextMock = new();
    protected readonly Mock<ICacheService> CacheMock = new();

    protected readonly CacheSettings CacheSettings = new()
    {
        ConnectionString = "localhost:6379",
        EventTtlSeconds = 300,
        TopEventsTtlSeconds = 60
    };

    protected readonly IServiceProvider ServiceProvider;

    protected BaseUnitTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton<TimeProvider>(new FakeTimeProvider());

        ConfigureMockServices(services);

        services.AddScoped<GetEventsByQueryHandler>();
        services.AddScoped<GetEventByIdQueryHandler>();
        services.AddScoped<GetTopEventsQueryHandler>();
        services.AddScoped<CreateEventCommandHandler>();
        services.AddScoped<UpdateEventCommandHandler>();
        services.AddScoped<DeleteEventCommandHandler>();

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureMockServices(IServiceCollection services)
    {
        var @event = Event.Create(EventId, EventTitle, EventDescription,
            EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);

        UserContextMock.Setup(x => x.UserId).Returns(UserId);
        UserContextMock.Setup(x => x.IsAuthenticated).Returns(true);

        EventRepositoryMock
            .Setup(repo => repo.GetFiltered(3, 15, new Filters(Title: "День", EventStartAt, EventEndAt),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<Event>(Data: [@event], TotalItems: 100));
        EventRepositoryMock.Setup(repo => repo.Find(EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(@event);
        EventRepositoryMock.Setup(repo => repo.Add(@event));
        EventRepositoryMock.Setup(repo => repo.Delete(@event));
        EventRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        CacheMock
            .Setup(cache => cache.GetAsync<CachedEvent>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedEvent?)null);
        CacheMock
            .Setup(cache => cache.GetAsync<CachedEvent[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedEvent[]?)null);

        services.AddSingleton(UserContextMock.Object);
        services.AddSingleton(EventRepositoryMock.Object);
        services.AddSingleton(CacheSettings);
        services.AddSingleton(CacheMock.Object);
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}