using Events.Application.Contracts.Commands.Events;
using Events.Application.UseCases.Events;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.CommandHandlers.Events;

public class AddEventCommandHandlerTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Add_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<AddEventCommandHandler>();
		var command = new AddEventCommand(EventTitle, EventDescription, EventStartAt, EventEndAt, EventTotalSeats);

		//Act
		var result = await handler.Handle(command, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Add(It.Is<Event>(x => x.Title == EventTitle && x.Description == EventDescription)),
			Times.Once);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);

		result.Should().NotBe(Guid.Empty);
	}
	
	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Add_WhenInvalidData_ShouldThrowArgumentNullException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<AddEventCommandHandler>();
		var command = new AddEventCommand("8 марта", "Международный женский день", default, default, 100);

		//Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);
		await act.Should().ThrowAsync<ArgumentNullException>();

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Event>()),
			Times.Never);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}
}