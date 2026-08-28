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

public class CancelBookingCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет отмену брони.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        var command = new CancelBookingCommand(BookingId);

        //Act
        await handler.Handle(command, CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }

    /// <summary>
    /// Проверяет отмену несуществующей брони.
    /// </summary>
    [Fact]
    public async Task Handle_WhenBookingDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        var nonExistentBookingId = Guid.NewGuid();
        var command = new CancelBookingCommand(nonExistentBookingId);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Бронь] с идентификатором [{nonExistentBookingId.ToString()}] не найдена.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == nonExistentBookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет отмену брони на несуществующее событие.
    /// </summary>
    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        var nonExistentEventId = Guid.NewGuid();
        BookingRepositoryMock.Setup(repo => repo.Find(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Booking.Create(BookingId, nonExistentEventId, UserId, DateTime.UtcNow));
        var command = new CancelBookingCommand(BookingId);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Событие] с идентификатором [{nonExistentEventId.ToString()}] не найдена.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет отмену чужой брони обычным пользователем.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ShouldThrowAccessDeniedException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        //UserContextMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        //UserContextMock.Setup(x => x.IsAdmin).Returns(false);
        var command = new CancelBookingCommand(BookingId);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<AccessDeniedException>()
            .WithMessage("Недостаточно прав.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Проверяет отмену чужой брони администратором.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsAdmin_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        //UserContextMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
        //UserContextMock.Setup(x => x.IsAdmin).Returns(true);
        var command = new CancelBookingCommand(BookingId);

        //Act
        await handler.Handle(command, CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }

    /// <summary>
    /// Проверяет отмену брони, которая уже не в статусе Pending.
    /// </summary>
    [Fact]
    public async Task Handle_WhenBookingIsConfirmed_ShouldThrowBookingMustBeInPendingStatusException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<CancelBookingCommandHandler>();
        var booking = Booking.Create(BookingId, EventId, UserId, DateTime.UtcNow);
        booking.Confirm(DateTime.UtcNow);
        BookingRepositoryMock.Setup(repo => repo.Find(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
        var command = new CancelBookingCommand(BookingId);

        //Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BookingMustBeInPendingStatusException>()
            .WithMessage("Нельзя изменить бронирование. Бронирование отменено.");

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
