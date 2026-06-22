using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using Events.Infrastructure;
using Events.Infrastructure.DataAccess;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Events.UnitTests.Application;

public class EventServiceUnitTests : BaseUnitTest
{
	private readonly AppDbContext _context;
	private readonly EventService _service;

	public EventServiceUnitTests()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new AppDbContext(options);
		_service = new EventService(new EventRepository(_context));

		SeedDatabase(options);
	}

	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var dto = new EventDto(eventId, "8 марта", "Международный женский день",
			Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2), 100);

		//Act
		var result = await _service.Add(dto, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.Title.Should().Be(dto.Title);
		(await _service.GetById(eventId, CancellationToken.None)).Should().BeEquivalentTo(dto);
	}

	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Add_InvalidData_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var dto = new EventDto(eventId, "8 марта", "Международный женский день", default, default, 100);

		//Act
		Func<Task> act = () => _service.Add(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<ArgumentNullException>();
		Func<Task> act2 = () => _service.GetById(eventId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Update_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto(EventId1, "8 марта", "Международный женский день",
			Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2), 10);

		//Act
		await _service.Update(dto, CancellationToken.None);

		//Assert
		(await _service.GetById(EventId1, CancellationToken.None)).Should().BeEquivalentTo(dto);
	}

	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Update_InvalidData_Failed()
	{
		//Arrange
		var dto = new EventDto(EventId1, "8 марта", "Международный женский день", Now, Now.AddDays(-1), 10);

		//Act
		Func<Task> act = () => _service.Update(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<ArgumentException>()
			.WithMessage("Начало события должно быть раньше его завершения.");
		var @event = await _service.GetById(EventId1, CancellationToken.None);
		@event.Title.Should().Be("День рождения");
		@event.Description.Should().Be("Дед Мороз и снегурочка");
		@event.StartAt.Should().Be(Now);
		@event.EndAt.Should().Be(Now.AddDays(7));
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Update_NonExistentEvent_Failed()
	{
		//Arrange
		var dto = new EventDto(Guid.NewGuid(), "8 марта", "Международный женский день",
			Now, Now.AddDays(-1), 100);

		//Act
		Func<Task> act = () => _service.Update(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{dto.Id.ToString()}] не найдена.");
		(await _service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(5);
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public async Task Remove_ValidData_Success()
	{
		//Act
		await _service.Remove(EventId1, CancellationToken.None);

		//Assert
		(await _service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(4);
		Func<Task> act = () => _service.GetById(EventId1, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{EventId1.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет удаление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Remove_NonExistentEvent_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();

		//Act
		Func<Task> act = () => _service.Remove(eventId, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		(await _service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(5);
	}

	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		//Act
		var result = await _service.GetById(EventId1, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("День рождения");
		result.Description.Should().Be("Дед Мороз и снегурочка");
		result.StartAt.Should().Be(Now);
		result.EndAt.Should().Be(Now.AddDays(7));
	}

	/// <summary>
	/// Проверяет получение несуществующего события.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentEvent_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();

		//Act
		Func<Task> act = () => _service.GetById(eventId, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет получение всех событий.
	/// </summary>
	[Fact]
	public async Task GetBy_ValidData_Success()
	{
		//Act
		var result = await _service.GetBy(new Filters(), 1, 10, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.Items.Should().HaveCount(5);
	}

	/// <summary>
	/// Проверяет фильтрацию по наименованию.
	/// </summary>
	[Theory]
	[InlineData("23")]
	[InlineData("23 февраля")]
	[InlineData("фев")]
	[InlineData("вра")]
	[InlineData("аля")]
	[InlineData("ФеВрАлЯ")]
	public async Task GetBy_FilterByTitle_Success(string title)
	{
		//Act
		var result = await _service.GetBy(new Filters(Title: title), 1, 10, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.ItemsPerPage.Should().Be(1);
		var item = result.Items.Should().ContainSingle().Subject;
		item.Title.Should().Be("23 февраля");
	}

	/// <summary>
	/// Проверяет фильтрацию по дате начала события.
	/// </summary>
	[Theory]
	[InlineData(0, 2)]
	[InlineData(-6, 3)]
	public async Task GetBy_FilterByFrom_Success(int daysToAdd, int totalItems)
	{
		//Act
		var result = await _service.GetBy(new Filters(From: Now.AddDays(daysToAdd)), 1, 10, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Items.Should().OnlyContain(item => item.StartAt >= Now.AddDays(daysToAdd));
	}

	/// <summary>
	/// Проверяет фильтрацию по дате окончания события.
	/// </summary>
	[Theory]
	[InlineData(0, 3)]
	[InlineData(-5, 0)]
	public async Task GetBy_FilterByTo_Success(int monthToAdd, int totalItems)
	{
		//Act
		var result = await _service.GetBy(new Filters(To: Now.AddMonths(monthToAdd)), 1, 10, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Items.Should().OnlyContain(item => item.EndAt <= Now.AddMonths(monthToAdd));
	}

	/// <summary>
	/// Проверяет комбинированную фильтрацию.
	/// </summary>
	[Theory]
	[InlineData(6, 1)]
	[InlineData(8, 2)]
	public async Task GetBy_CombinedFilterBy_Success(int endAtAddDays, int filteredItems)
	{
		//Act
		var result = await _service.GetBy(new Filters(Title: "День", Now.AddDays(-1),
			Now.AddDays(endAtAddDays)), 1, 10, CancellationToken.None);

		//Assert
		result.Items.Should().HaveCount(filteredItems);
	}

	/// <summary>
	/// Проверяет пагинацию событий.
	/// </summary>
	[Theory]
	[InlineData(1, 3, 3)]
	[InlineData(2, 3, 2)]
	[InlineData(3, 3, 0)]
	[InlineData(1, 2, 2)]
	[InlineData(2, 2, 2)]
	[InlineData(3, 2, 1)]
	public async Task GetBy_Pagination_Success(int page, int pageSize, int itemsPerPage)
	{
		//Act
		var result = await _service.GetBy(new Filters(), page, pageSize, CancellationToken.None);

		//Assert
		result.TotalItems.Should().Be(5);
		result.CurrentPage.Should().Be(page);
		result.ItemsPerPage.Should().Be(itemsPerPage);
		result.Items.Should().HaveCount(itemsPerPage);
	}

	public override void Dispose()
	{
		_context.Dispose();
	}
}