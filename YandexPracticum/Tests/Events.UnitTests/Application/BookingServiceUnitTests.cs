using System.Collections.Concurrent;
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
	private const int EventTotalSeats = 10;
	private readonly IEventRepository _eventRepository = new InMemoryEventStore();
	private readonly IBookingRepository _bookingRepository = new InMemoryBookingStore();
	private readonly IBookingService _service;

	public BookingServiceUnitTests()
	{
		var now = DateTime.UtcNow;

		_eventRepository.Add(Event.Create(_eventId1, "День рождения", "Дед Мороз и снегурочка",
			EventPeriod.Create(now, now.AddDays(7)), EventTotalSeats));
		_eventRepository.Add(Event.Create(_eventId2, "Пасха", "Красим яйца, печем куличи",
			EventPeriod.Create(now.AddHours(-12), now.AddHours(-10)), 20));
		_eventRepository.Add(Event.Create(_eventId3, "День победы", "Парад и салют",
			EventPeriod.Create(now, now.AddHours(14)), 100));

		_service = new BookingService(_bookingRepository, _eventRepository);
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
		_eventRepository.Find(_eventId1)!.AvailableSeats.Should().Be(EventTotalSeats - 1);
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
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public void Add_NoAvailableSeats_ExceptionThrown()
	{
		//Arrange
		for (var i = 0; i < EventTotalSeats; i++)
		{
			_service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId1));
		}

		//Act
		Action act = () => _service.Add(new BookingToAddDto(_bookingId, _eventId1));

		//Assert
		act.Should().Throw<NoAvailableSeatsException>()
			.WithMessage("No available seats for this event.");
		Action act2 = () => _service.GetById(_bookingId);
		act2.Should().Throw<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}
	
	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Success()
	{
		// Arrange
		var successCount = 0;
		var exceptionsCount = 0;
		var bookingIdsList = new ConcurrentBag<Guid>();

		//Act
		var tasks = Enumerable.Range(0, EventTotalSeats)
			.Select(_ => Task.Run(() =>
			{
				try
				{
					var bookingId = Guid.NewGuid();
					bookingIdsList.Add(bookingId);
					_service.Add(new BookingToAddDto(bookingId, _eventId1));
					Interlocked.Increment(ref successCount);
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		successCount.Should().Be(EventTotalSeats);
		exceptionsCount.Should().Be(0);
		_eventRepository.Find(_eventId1)!.AvailableSeats.Should().Be(0);
		bookingIdsList.Distinct().Should().HaveCount(successCount);
	}

	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события при овербукинге.
	/// </summary>
	[Fact]
	public async Task Add_MultipleBookingsForOneEvent_Overbooking_Success()
	{
		// Arrange
		var totalRequests = 25;
		var successCount = 0;
		var exceptionsCount = 0;

		//Act
		var tasks = Enumerable.Range(0, totalRequests)
			.Select(_ => Task.Run(() =>
			{
				try
				{
					_service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId1));
					Interlocked.Increment(ref successCount);
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		successCount.Should().Be(EventTotalSeats);
		exceptionsCount.Should().Be(totalRequests - EventTotalSeats);
		_eventRepository.Find(_eventId1)!.AvailableSeats.Should().Be(0);
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