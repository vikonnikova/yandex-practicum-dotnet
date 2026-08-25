using Events.Application.Contracts.Commands.Bookings;
using Events.Application.UseCases.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.CommandHandlers.Bookings;

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

        EventRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
            Times.Once);

        BookingRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(CancellationToken.None),
            Times.Once);
    }
}