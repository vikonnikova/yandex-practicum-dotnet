using Events.Application.Contracts.Queries;
using Events.Application.Exceptions;
using Events.Application.QueryHandlers;
using Events.Application.Settings;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.QueryHandlers;

public class GetEventByIdQueryHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет получение события по идентификатору при промахе кеша.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();

        //Act
        var result = await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == EventId), It.IsAny<CancellationToken>()),
            Times.Once);

        CacheMock.Verify(
            cache => cache.SetAsync(
                CacheKeys.Event(EventId),
                It.Is<CachedEvent>(x => x.Id == EventId && x.Title == EventTitle),
                TimeSpan.FromSeconds(CacheSettings.EventTtlSeconds),
                It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNull();
        result.Title.Should().Be(EventTitle);
        result.Description.Should().Be(EventDescription);
        result.Period.StartAt.Should().Be(EventStartAt);
        result.Period.EndAt.Should().Be(EventEndAt);
        result.TotalSeats.Should().Be(EventTotalSeats);
        result.AvailableSeats.Should().Be(EventTotalSeats);
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
        cached.TryReserveSeats(2);

        CacheMock
            .Setup(cache => cache.GetAsync<CachedEvent>(CacheKeys.Event(EventId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CachedEvent.FromDomain(cached));

        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();

        //Act
        var result = await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.Find(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

        CacheMock.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<CachedEvent>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        result.AvailableSeats.Should().Be(EventTotalSeats - 2);
        result.Title.Should().Be(EventTitle);
    }

    /// <summary>
    /// Проверяет получение несуществующего события.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNonExistentEvent_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();
        var eventId = Guid.NewGuid();

        //Act
        Func<Task> act = () => handler.Handle(new GetEventByIdQuery(eventId), CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == eventId), It.IsAny<CancellationToken>()),
            Times.Once);

        CacheMock.Verify(
            cache => cache.SetAsync(
                It.IsAny<string>(),
                It.IsAny<CachedEvent>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
