using Events.Application.Contracts.Queries.Events;
using Events.Application.Exceptions;
using Events.Application.QueryHandlers.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application.QueryHandlers.Events;

public class GetEventByIdQueryHandlerTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_WhenValidData_ShouldWorkCorrectly()
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

		result.Should().NotBeNull();
		result.Title.Should().Be(EventTitle);
		result.Description.Should().Be(EventDescription);
		result.Period.StartAt.Should().Be(EventStartAt);
		result.Period.EndAt.Should().Be(EventEndAt);
		result.TotalSeats.Should().Be(EventTotalSeats);
		result.AvailableSeats.Should().Be(EventTotalSeats);
	}

	/// <summary>
	/// Проверяет получение несуществующего события.
	/// </summary>
	[Fact]
	public async Task GetById_WhenNonExistentEvent_ShouldThrowEntityNotFoundException()
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
	}
}