using Events.Application.Exceptions;
using Events.Application.Interfaces;
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
	private readonly IEventRepository _eventRepository = new InMemoryEventStore();
	private readonly IBookingService _service;

	public BookingServiceUnitTests()
	{
		var now = DateTime.UtcNow;
		
		_eventRepository.Add(Event.Create(_eventId1, "День рождения", "Дед Мороз и снегурочка",
			EventPeriod.Create(now, now.AddDays(7)), 10));
		_eventRepository.Add(Event.Create(_eventId2, "Пасха", "Красим яйца, печем куличи",
			EventPeriod.Create(now.AddHours(-12), now.AddHours(-10)), 20));
		_eventRepository.Add(Event.Create(_eventId3, "День победы", "Парад и салют",
			EventPeriod.Create(now, now.AddHours(14)), 100));

		_service = new BookingService(new InMemoryBookingStore(), _eventRepository, new InMemoryBookingTaskQueue());
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
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public void Add_MultipleBookingsForOneEvent_Success()
	{
		//Arrange
		var bookingId1 = Guid.NewGuid();
		var bookingId2 = Guid.NewGuid();
		var bookingId3 = Guid.NewGuid();
		
		var bookings = new List<BookingToAddDto>
		{
			new(bookingId1, _eventId1), 
			new(bookingId2, _eventId1), 
			new(bookingId3, _eventId1)
		};

		//Act
		foreach (var booking in bookings)
		{
			_service.Add(booking);
		}

		//Assert
		foreach (var bookingId in new List<Guid> { bookingId1, bookingId2, bookingId3 })
		{
			var result = _service.GetById(bookingId);
			result.Should().NotBeNull();
			result.BookingId.Should().Be(bookingId);
			result.EventId.Should().Be(_eventId1);
			result.Status.Should().Be(BookingStatus.Pending);
		}
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public void Add_ForNonExistentEvent_Failed()
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
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public void Add_ForDeletedEvent_Failed()
	{
		//Arrange
		_service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId2));
		new EventService(_eventRepository).Remove(_eventId2);

		//Act
		Action act = () => _service.Add(new BookingToAddDto(_bookingId, _eventId2));

		//Assert
		act.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{_eventId2.ToString()}] не найдена.");
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