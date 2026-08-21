using Events.Application;
using Events.Application.Exceptions;
using Events.Application.Services;
using Events.Application.Services.Dto;
using Events.Domain;
using Events.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests.Application;

public class EventServiceUnitTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Add_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var dto = new EventDto(EventTitle, EventDescription, EventStartAt, EventEndAt, EventTotalSeats);

		//Act
		var result = await service.Add(dto, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Add(It.Is<Event>(x => x.Title == EventTitle && x.Description == EventDescription)),
			Times.Once);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);

		result.Should().NotBeNull();
		result.Title.Should().Be(EventTitle);
		result.Description.Should().Be(EventDescription);
		result.StartAt.Should().Be(EventStartAt);
		result.EndAt.Should().Be(EventEndAt);
		result.TotalSeats.Should().Be(EventTotalSeats);
		result.AvailableSeats.Should().Be(EventTotalSeats);
	}

	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Add_WhenInvalidData_ShouldThrowArgumentNullException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var dto = new EventDto("8 марта", "Международный женский день", default, default, 100);

		//Act
		Func<Task> act = () => service.Add(dto, CancellationToken.None);
		await act.Should().ThrowAsync<ArgumentNullException>();

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Event>()),
			Times.Never);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Update_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var dto = new EventToUpdateDto(EventId, "8 марта", "Международный женский день",
			EventStartAt, EventEndAt, 10);

		//Act
		await service.Update(dto, CancellationToken.None);

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
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var eventId = Guid.NewGuid();
		var dto = new EventToUpdateDto(eventId, "8 марта", "Международный женский день",
			EventStartAt, EventEndAt, 100);

		//Act
		Func<Task> act = () => service.Update(dto, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{dto.Id.ToString()}] не найдена.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == eventId), It.IsAny<CancellationToken>()),
			Times.Once);

		EventRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public async Task Remove_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		await service.Remove(EventId, CancellationToken.None);

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
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var eventId = Guid.NewGuid();

		//Act
		Func<Task> act = () => service.Remove(eventId, CancellationToken.None);
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

	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetById(EventId, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), It.IsAny<CancellationToken>()),
			Times.Once);

		result.Should().NotBeNull();
		result.Title.Should().Be(EventTitle);
		result.Description.Should().Be(EventDescription);
		result.StartAt.Should().Be(EventStartAt);
		result.EndAt.Should().Be(EventEndAt);
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
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var eventId = Guid.NewGuid();

		//Act
		Func<Task> act = () => service.GetById(eventId, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == eventId), It.IsAny<CancellationToken>()),
			Times.Once);
	}

	/// <summary>
	/// Проверяет получение событий с пагинацией и фильтрацией.
	/// </summary>
	[Fact]
	public async Task GetBy_WhenFiltersAndPaginationProvided_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();
		var filters = new Filters(Title: "День", EventStartAt, EventEndAt);

		//Act
		var result = await service.GetBy(Page, PageSize, filters, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.GetFiltered(Page, PageSize,
				It.Is<Filters>(x => x.Title == filters.Title && x.From == filters.From && x.To == filters.To),
				CancellationToken.None), Times.Once);

		result.Should().NotBeNull();
		result.TotalItems.Should().Be(100);
		result.CurrentPage.Should().Be(3);

		var item = result.Items.Should().ContainSingle().Subject;
		item.Title.Should().Be(EventTitle);
		item.Description.Should().Be(EventDescription);
		item.StartAt.Should().Be(EventStartAt);
		item.EndAt.Should().Be(EventEndAt);
		item.TotalSeats.Should().Be(EventTotalSeats);
		item.AvailableSeats.Should().Be(EventTotalSeats);
	}
}