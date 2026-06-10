using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using Events.Domain;
using Events.Infrastructure;
using FluentAssertions;

namespace Events.UnitTests.Application;

public class BookingServiceUnitTests
{
	private readonly Guid _eventId1 = Guid.NewGuid();
	private readonly Guid _eventId2 = Guid.NewGuid();
	private readonly Guid _eventId3 = Guid.NewGuid();
	private readonly Guid _bookingId = Guid.NewGuid();
	private readonly IBookingService _service;

	public BookingServiceUnitTests()
	{
		var now = DateTime.UtcNow;
		var eventRepository = new InMemoryEventStore();
		eventRepository.Add(Event.Create(_eventId1, "День рождения", "Дед Мороз и снегурочка",
			EventPeriod.Create(now, now.AddDays(7))));
		eventRepository.Add(Event.Create(_eventId2, "Пасха", "Красим яйца, печем куличи",
			EventPeriod.Create(now.AddHours(-12), now.AddHours(-10))));
		eventRepository.Add(Event.Create(_eventId3, "День победы", "Парад и салют",
			EventPeriod.Create(now, now.AddHours(14))));

		_service = new BookingService(new InMemoryBookingStore(), eventRepository, new InMemoryBookingTaskQueue());
		_service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId2));
		_service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId3));
	}

	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public void Add_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);

		//Act
		var result = _service.Add(dto);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(_bookingId);
		result.EventId.Should().Be(_eventId1);
		result.Status.Should().Be(BookingStatus.Pending);
		_service.GetById(_bookingId).Should().BeEquivalentTo(dto);
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public void Add_NonExistentEvent_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var dto = new BookingToAddDto(_bookingId, eventId);

		//Act
		Action act = () => _service.Add(dto);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		Action act2 = () => _service.GetById(_bookingId);
		act2.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public void Confirm_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);
		_service.Add(dto);

		//Act
		_service.Confirm(_bookingId);

		//Assert
		var result = _service.GetById(_bookingId);
		result.Should().NotBeNull();
		result.BookingId.Should().Be(_bookingId);
		result.EventId.Should().Be(_eventId1);
		result.Status.Should().Be(BookingStatus.Confirmed);
	}

	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public void Confirm_NonExistentBooking_Failed()
	{
		//Act
		Action act = () => _service.Confirm(_bookingId);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет удаление события.
	/// </summary>
	[Fact]
	public void Reject_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);
		_service.Add(dto);

		//Act
		_service.Reject(_bookingId);

		//Assert
		var result = _service.GetById(_bookingId);
		result.Should().NotBeNull();
		result.BookingId.Should().Be(_bookingId);
		result.EventId.Should().Be(_eventId1);
		result.Status.Should().Be(BookingStatus.Rejected);
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public void Reject_NonExistentBooking_Failed()
	{
		//Act
		Action act = () => _service.Reject(_bookingId);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public void GetById_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);
		_service.Add(dto);

		//Act
		var result = _service.GetById(_bookingId);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(_bookingId);
		result.EventId.Should().Be(_eventId1);
		result.Status.Should().Be(BookingStatus.Pending);
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public void GetById_NonExistentBooking_Failed()
	{
		//Act
		Action act = () => _service.GetById(_bookingId);

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}
}