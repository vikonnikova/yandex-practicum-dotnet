using Events.Application.Contracts.Queries;
using Events.Application.QueryHandlers;
using Events.Application.Settings;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.QueryHandlers;

public class GetTopEventsQueryHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет, что при промахе кеша данные берутся из репозитория и сохраняются в кеш.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCacheMiss_ShouldLoadFromRepositoryAndSetCache()
    {
        //Arrange
        var @event = Event.Create(EventId, EventTitle, EventDescription,
            EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);
        @event.TryReserveSeats(3);

        EventRepositoryMock
            .Setup(repo => repo.GetTopBySoldPercentage(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([@event]);

        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetTopEventsQueryHandler>();

        //Act
        var result = await handler.Handle(new GetTopEventsQuery(), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.GetTopBySoldPercentage(10, It.IsAny<CancellationToken>()),
            Times.Once);

        CacheMock.Verify(
            cache => cache.SetAsync(
                CacheKeys.Top10,
                It.Is<CachedEvent[]>(items => items.Length == 1 && items[0].Id == EventId),
                TimeSpan.FromSeconds(CacheSettings.TopEventsTtlSeconds),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().ContainSingle();
        result.First().AvailableSeats.Should().Be(EventTotalSeats - 3);
    }

    /// <summary>
    /// Проверяет, что при попадании в кеш репозиторий не вызывается.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCacheHit_ShouldNotCallRepository()
    {
        //Arrange
        var cached = Event.Create(EventId, EventTitle, EventDescription,
            EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);

        CacheMock
            .Setup(cache => cache.GetAsync<CachedEvent[]>(CacheKeys.Top10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([CachedEvent.FromDomain(cached)]);

        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetTopEventsQueryHandler>();

        //Act
        var result = await handler.Handle(new GetTopEventsQuery(), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.GetTopBySoldPercentage(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        CacheMock.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<CachedEvent[]>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.Should().ContainSingle(item => item.Id == EventId && item.Title == EventTitle);
    }
}
