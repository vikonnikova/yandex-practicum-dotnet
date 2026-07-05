using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Events.UnitTests.Application;

public class EventServiceUnitTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto("8 марта", "Международный женский день",
			Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2), 100);
		EventInfoDto returnedResult;

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			returnedResult = await service.Add(dto, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			returnedResult.Should().NotBeNull();
			returnedResult.Title.Should().Be(dto.Title);
			returnedResult.Description.Should().Be(dto.Description);
			returnedResult.StartAt.Should().Be(dto.StartAt);
			returnedResult.EndAt.Should().Be(dto.EndAt);

			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			var result = await service.GetById(returnedResult.Id, CancellationToken.None);

			result.Should().NotBeNull();
			result.Title.Should().Be(dto.Title);
			result.Description.Should().Be(dto.Description);
			result.StartAt.Should().Be(dto.StartAt);
			result.EndAt.Should().Be(dto.EndAt);
		}
	}

	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Add_InvalidData_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var dto = new EventDto("8 марта", "Международный женский день", default, default, 100);

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			Func<Task> act = () => service.Add(dto, CancellationToken.None);
			await act.Should().ThrowAsync<ArgumentNullException>();
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			Func<Task> act2 = () => service.GetById(eventId, CancellationToken.None);
			await act2.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		}
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Update_ValidData_Success()
	{
		//Arrange
		var dto = new EventToUpdateDto(EventId1, "8 марта", "Международный женский день",
			Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2), 10);

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			await service.Update(dto, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			var result = await service.GetById(EventId1, CancellationToken.None);
			result.Id.Should().NotBeEmpty();
			result.Title.Should().Be(dto.Title);
			result.Description.Should().Be(dto.Description);
			result.StartAt.Should().Be(dto.StartAt);
			result.EndAt.Should().Be(dto.EndAt);
			result.TotalSeats.Should().Be(dto.TotalSeats);
			result.AvailableSeats.Should().Be(dto.TotalSeats);
		}
	}

	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Update_InvalidData_Failed()
	{
		//Arrange
		var dto = new EventToUpdateDto(EventId1, "8 марта", "Международный женский день", Now, Now.AddDays(-1), 10);

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			Func<Task> act = () => service.Update(dto, CancellationToken.None);
			await act.Should().ThrowAsync<ArgumentException>()
				.WithMessage("Начало события должно быть раньше его завершения.");
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			var @event = await service.GetById(EventId1, CancellationToken.None);
			@event.Title.Should().Be("День рождения");
			@event.Description.Should().Be("Дед Мороз и снегурочка");
			@event.StartAt.Should().Be(Now);
			@event.EndAt.Should().Be(Now.AddDays(7));
		}
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Update_NonExistentEvent_Failed()
	{
		//Arrange
		var dto = new EventToUpdateDto(Guid.NewGuid(), "8 марта", "Международный женский день",
			Now, Now.AddDays(-1), 100);

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			Func<Task> act = () => service.Update(dto, CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{dto.Id.ToString()}] не найдена.");
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(5);
		}
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public async Task Remove_ValidData_Success()
	{
		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			await service.Remove(EventId1, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(4);
			Func<Task> act = () => service.GetById(EventId1, CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{EventId1.ToString()}] не найдена.");
		}
	}

	/// <summary>
	/// Проверяет удаление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Remove_NonExistentEvent_Failed()
	{
		using (var scope = ServiceProvider.CreateScope())
		{
			//Arrange
			var eventId = Guid.NewGuid();
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();

			//Act
			Func<Task> act = () => service.Remove(eventId, CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		}

		using (var scope = ServiceProvider.CreateScope())
		{
			//Assert
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await service.GetBy(new Filters(), 1, 10, CancellationToken.None)).Items.Should().HaveCount(5);
		}
	}

	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetById(EventId1, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var eventId = Guid.NewGuid();
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		Func<Task> act = () => service.GetById(eventId, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(), 1, 10, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(Title: title), 1, 10, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(From: Now.AddDays(daysToAdd)), 1, 10, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(To: Now.AddMonths(monthToAdd)), 1, 10, CancellationToken.None);

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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(Title: "День", Now.AddDays(-1),
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
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IEventService>();

		//Act
		var result = await service.GetBy(new Filters(), page, pageSize, CancellationToken.None);

		//Assert
		result.TotalItems.Should().Be(5);
		result.CurrentPage.Should().Be(page);
		result.ItemsPerPage.Should().Be(itemsPerPage);
		result.Items.Should().HaveCount(itemsPerPage);
	}
}