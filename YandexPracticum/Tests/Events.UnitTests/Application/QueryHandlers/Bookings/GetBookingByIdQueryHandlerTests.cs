using Events.Application.Contracts.Queries.Bookings;
using Events.Application.Exceptions;
using Events.Application.QueryHandlers.Bookings;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.QueryHandlers.Bookings;

public class GetBookingByIdQueryHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет получение брони по идентификатору.
    /// </summary>
    [Fact]
    public async Task GetById_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetBookingByIdQueryHandler>();

        //Act
        var result = await handler.Handle(new GetBookingByIdQuery(BookingId), CancellationToken.None);

        //Assert
        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == BookingId), CancellationToken.None),
            Times.Once);

        result.Should().NotBeNull();
        result.Id.Should().Be(BookingId);
        result.EventId.Should().Be(EventId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Проверяет получение несуществующей брони.
    /// </summary>
    [Fact]
    public async Task GetById_WhenBookingDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<GetBookingByIdQueryHandler>();
        var nonExistentBookingId = Guid.NewGuid();

        //Act
        Func<Task> act = () => handler.Handle(new GetBookingByIdQuery(nonExistentBookingId), CancellationToken.None);

        //Assert
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Бронь] с идентификатором [{nonExistentBookingId.ToString()}] не найдена.");

        BookingRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == nonExistentBookingId), CancellationToken.None),
            Times.Once);
    }
}