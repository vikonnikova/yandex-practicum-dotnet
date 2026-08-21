using Events.Application.Contracts.Commands.Events;
using Events.Application.Exceptions;
using Events.Application.UseCases.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.CommandHandlers.Events;

public class UpdateEventCommandHandlerTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Update_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<UpdateEventCommandHandler>();
		var command = new UpdateEventCommand(EventId, "8 марта", "Международный женский день",
			EventStartAt, EventEndAt, 10);

		//Act
		await handler.Handle(command, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), It.IsAny<CancellationToken>()),
			Times.Once);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Update_WhenEventDoesNotExist_ShouldThrowEntityNotFoundException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<UpdateEventCommandHandler>();
		var eventId = Guid.NewGuid();
		var command = new UpdateEventCommand(eventId, "8 марта", "Международный женский день",
			EventStartAt, EventEndAt, 100);

		//Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{command.Id.ToString()}] не найдена.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == eventId), It.IsAny<CancellationToken>()),
			Times.Once);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}
}