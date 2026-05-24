using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using FluentAssertions;

namespace Events.UnitTests.Application;

public class EventServiceUnitTests
{
	private readonly DateTime _now = DateTime.UtcNow;
	private readonly EventService _service = new();

	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public void Add_ValidData_Success()
	{
		//Arrange
		var dto = new EventDto(1, "8 марта", "Международный женский день",
			_now.AddMonths(-5), _now.AddMonths(-5).AddDays(2));

		//Act
		var result = _service.Add(dto);

		//Assert
		result.Should().NotBeNull();
		result.Title.Should().Be(dto.Title);
		_service.GetById(1).Should().BeEquivalentTo(dto);
	}

	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public void Add_InvalidData_Failed()
	{
		//Arrange
		var dto = new EventDto(1, "8 марта", "Международный женский день", default, default);

		//Act
		Action act = () => _service.Add(dto);

		//Assert
		act.Should().Throw<ArgumentNullException>();
		//TODO проверить, что событие не создалось
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public void Update_ValidData_Success()
	{
		//Arrange
		CreateEvents();
		var dto = new EventDto(1, "8 марта", "Международный женский день",
			_now.AddMonths(-5), _now.AddMonths(-5).AddDays(2));

		//Act
		_service.Update(dto);

		//Assert
		_service.GetById(1).Should().BeEquivalentTo(dto);
	}

	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public void Update_InvalidData_Failed()
	{
		//Arrange
		CreateEvents();
		var dto = new EventDto(1, "8 марта", "Международный женский день",
			_now, _now.AddDays(-1));

		//Act
		Action act = () => _service.Update(dto);

		//Assert
		act.Should().Throw<ArgumentException>().WithMessage("Начало события должно быть раньше его завершения.");
		//TODO проверить, что событие не обновилось
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public void Update_NonExistentEvent_Failed()
	{
		//Arrange
		var dto = new EventDto(10, "8 марта", "Международный женский день", _now, _now.AddDays(-1));

		//Act
		Action act = () => new EventService().Update(dto);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage("Сущность [Событие] с идентификатором [10] не найдена.");
		//TODO проверить, что события не изменились (общее количество)
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public void Remove_ValidData_Success()
	{
		//Arrange
		CreateEvents();

		//Act
		_service.Remove(2);

		//Assert
		_service.GetBy(new Filters(), 1, 10).Items.Count.Should().Be(4);
		Action act = () => _service.GetById(2);
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage("Сущность [Событие] с идентификатором [2] не найдена.");
	}

	/// <summary>
	/// Проверяет удаление несуществующего события.
	/// </summary>
	[Fact]
	public void Remove_NonExistentEvent_Failed()
	{
		//Arrange
		CreateEvents();

		//Act
		Action act = () => _service.Remove(10);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage("Сущность [Событие] с идентификатором [10] не найдена.");
		//TODO проверить, что события не изменились (общее количество)
	}

	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public void GetById_ValidData_Success()
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetById(3);

		//Assert
		result.Should().NotBeNull();
		result.Title.Should().Be("Пасха");
	}

	/// <summary>
	/// Проверяет получение всех событий.
	/// </summary>
	[Fact]
	public void GetBy_ValidData_Success()
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetBy(new Filters(), 1, 10);

		//Assert
		result.Should().NotBeNull();
		result.Items.Count.Should().Be(5);
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
	public void GetBy_FilterByTitle_Success(string title)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetBy(new Filters(Title: title), 1, 10);

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
	public void GetBy_FilterByFrom_Success(int daysToAdd, int totalItems)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetBy(new Filters(From: _now.AddDays(daysToAdd)), 1, 10);

		//Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Items.Should().OnlyContain(item => item.StartAt >= _now.AddDays(daysToAdd));
	}

	/// <summary>
	/// Проверяет фильтрацию по дате окончания события.
	/// </summary>
	[Theory]
	[InlineData(0, 3)]
	[InlineData(-5, 0)]
	public void GetBy_FilterByTo_Success(int monthToAdd, int totalItems)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetBy(new Filters(To: _now.AddMonths(monthToAdd)), 1, 10);

		//Assert
		result.Should().NotBeNull();
		result.TotalItems.Should().Be(totalItems);
		result.Items.Should().OnlyContain(item => item.EndAt <= _now.AddMonths(monthToAdd));
	}

	/// <summary>
	/// Проверяет комбинированную фильтрацию.
	/// </summary>
	[Theory]
	[InlineData(6, 1)]
	[InlineData(8, 2)]
	public void GetBy_CombinedFilterBy_Success(int endAtAddDays, int filteredItems)
	{
		//Arrange
		CreateEvents();

		//Act
		var result =
			_service.GetBy(
				new Filters(Title: "День", _now.AddDays(-1), _now.AddDays(endAtAddDays)), 1, 10);

		//Assert
		result.Items.Count.Should().Be(filteredItems);
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
	public void GetBy_Pagination_Success(int page, int pageSize, int itemsPerPage)
	{
		//Arrange
		CreateEvents();

		//Act
		var result = _service.GetBy(new Filters(), page, pageSize);

		//Assert
		result.TotalItems.Should().Be(5);
		result.CurrentPage.Should().Be(page);
		result.ItemsPerPage.Should().Be(itemsPerPage);
		result.Items.Count.Should().Be(itemsPerPage);
	}

	private void CreateEvents()
	{
		_service.Add(new EventDto(1, "День рождения", "Дед Мороз и снегурочка", _now, _now.AddDays(7)));
		_service.Add(new EventDto(2, "Рождество", "описание рождества, подарки, игрушки", _now.AddMonths(-5),
			_now.AddMonths(-5).AddDays(2)));
		_service.Add(new EventDto(3, "Пасха", "Красим яйца, печем куличи", _now.AddHours(-12), _now.AddHours(-10)));
		_service.Add(new EventDto(4, "23 февраля", "День защитника отечества", _now.AddDays(-7), _now.AddDays(-6)));
		_service.Add(new EventDto(5, "День победы", "Парад и салют", _now, _now.AddHours(14)));
	}
}