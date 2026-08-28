using System.Collections.Concurrent;
using Bookings.Application.Contracts.Commands;
using Bookings.Application.Exceptions;
using Bookings.Application.UseCases;
using Bookings.Domain;
using Bookings.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Bookings.UnitTests.Application.CommandHandlers;

public class CreateBookingCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет создание брони.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();
        var command = new CreateBookingCommand(EventId);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Add(It.Is<Booking>(x => x.EventId == EventId)),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        result.Should().NotBeNull();
        result.EventId.Should().Be(EventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Проверяет создание брони на несуществующее событие.
    /// </summary>
    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();
        var nonExistentEventId = Guid.NewGuid();
        var command = new CreateBookingCommand(nonExistentEventId);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Событие] с идентификатором [{nonExistentEventId.ToString()}] не найдена.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<Booking>()),
            Times.Never);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет создание брони при достижении лимита пользователем.
    /// </summary>
    [Fact]
    public async Task Handle_WhenBookingLimitReached_ShouldThrowBookingLimitReachingException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        BookingRepositoryMock.Setup(repo => repo.CountPendingByUser(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();

        //Act
        Func<Task> act = () => handler.Handle(new CreateBookingCommand(EventId), CancellationToken.None);
        await act.Should().ThrowAsync<BookingLimitReachingException>()
            .WithMessage("Достигнут лимит [10] бронирования у события.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<Booking>()),
            Times.Never);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет, что лимиты разных пользователей не влияют друг на друга.
    /// </summary>
    [Fact]
    public async Task Handle_WhenBookingLimitReachedForOtherUser_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        BookingRepositoryMock.Setup(repo => repo.CountPendingByUser(Guid.NewGuid(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);
        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();

        //Act
        var result = await handler.Handle(new CreateBookingCommand(EventId), CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.CountPendingByUser(It.Is<Guid>(x => x == UserId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.Add(It.IsAny<Booking>()),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
    /// </summary>
    [Fact]
    public async Task Handle_WhenMultipleBookingsForOneEvent_ShouldWorkCorrectly()
    {
        // Arrange
        var successCount = 0;
        var exceptionsCount = 0;
        var bookingIdsList = new ConcurrentBag<Guid>();

        //Act
        var tasks = Enumerable.Range(0, EventTotalSeats)
            .Select(async _ =>
            {
                try
                {
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var bookingId = Guid.NewGuid();
                        bookingIdsList.Add(bookingId);

                        var handler = scope.ServiceProvider.GetRequiredService<CreateBookingCommandHandler>();
                        await handler.Handle(new CreateBookingCommand(EventId), CancellationToken.None);

                        Interlocked.Increment(ref successCount);
                    }
                }
                catch
                {
                    Interlocked.Increment(ref exceptionsCount);
                }
            }).ToArray();

        await Task.WhenAll(tasks);

        //Assert
        successCount.Should().Be(EventTotalSeats);
        exceptionsCount.Should().Be(0);
        bookingIdsList.Distinct().Should().HaveCount(successCount);
    }
}