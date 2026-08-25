using Events.Application.Contracts.Commands.Events;
using Events.Application.Exceptions;
using Events.Application.UseCases.Events;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.CommandHandlers.Events;

public class RemoveEventCommandHandlerTests : BaseUnitTest
{
    /// <summary>
    /// Проверяет удаление события.
    /// </summary>
    [Fact]
    public async Task Remove_WhenValidData_ShouldWorkCorrectly()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RemoveEventCommandHandler>();

        //Act
        await handler.Handle(new RemoveEventCommand(EventId), CancellationToken.None);

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == EventId), It.IsAny<CancellationToken>()),
            Times.Once);

        EventRepositoryMock.Verify(
            repo => repo.Delete(
                It.Is<Event>(x => x.Title == EventTitle && x.Description == EventDescription)),
            Times.Once);

        EventRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Проверяет удаление несуществующего события.
    /// </summary>
    [Fact]
    public async Task Remove_WhenNonExistentEvent_ShouldThrowEntityNotFoundException()
    {
        //Arrange
        using var scope = ServiceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<RemoveEventCommandHandler>();
        var eventId = Guid.NewGuid();

        //Act
        Func<Task> act = () => handler.Handle(new RemoveEventCommand(eventId), CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>()
            .WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");

        //Assert
        EventRepositoryMock.Verify(
            repo => repo.Find(It.Is<Guid>(x => x == eventId), It.IsAny<CancellationToken>()),
            Times.Once);

        EventRepositoryMock.Verify(
            repo => repo.Delete(It.IsAny<Event>()),
            Times.Never);

        EventRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}