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
	private readonly IEventRepository _eventRepository = new EventRepository();
	private readonly IBookingRepository _bookingRepository = new BookingRepository();
	private readonly IBookingService _service;

	public BookingServiceUnitTests()
	{
		// TODO через контекст заполнить данными
		
		var now = DateTime.UtcNow;

		await _eventRepository.Add(Event.Create(_eventId1, "День рождения", "Дед Мороз и снегурочка",
			EventPeriod.Create(now, now.AddDays(7)), EventTotalSeats), CancellationToken.None);
		_eventRepository.Add(Event.Create(_eventId2, "Пасха", "Красим яйца, печем куличи",
			EventPeriod.Create(now.AddHours(-12), now.AddHours(-10)), 20), CancellationToken.None);
		_eventRepository.Add(Event.Create(_eventId3, "День победы", "Парад и салют",
			EventPeriod.Create(now, now.AddHours(14)), 100), CancellationToken.None);

		_service = new BookingService(_bookingRepository, _eventRepository);
		await _service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId2), CancellationToken.None);
		await _service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId3), CancellationToken.None);
	}

	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);

		//Act
		var result = await _service.Add(dto, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(_bookingId);
		result.EventId.Should().Be(_eventId1);
		result.Status.Should().Be(BookingStatus.Pending);
		(await _service.GetById(_bookingId, CancellationToken.None)).Should().BeEquivalentTo(dto);
		(await _eventRepository.Find(_eventId1, CancellationToken.None))!.AvailableSeats.Should()
			.Be(EventTotalSeats - 1);
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Add_ForNonExistentEvent_Failed()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var dto = new BookingToAddDto(_bookingId, eventId);

		//Act
		Func<Task> act = () => _service.Add(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		Func<Task> act2 = () => _service.GetById(_bookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Add_ForDeletedEvent_Failed()
	{
		//Arrange
		await _service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId2), CancellationToken.None);
		await new EventService(_eventRepository).Remove(_eventId2, CancellationToken.None);

		//Act
		Func<Task> act = () => _service.Add(new BookingToAddDto(_bookingId, _eventId2), CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{_eventId2.ToString()}] не найдена.");
		Func<Task> act2 = () => _service.GetById(_bookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public async Task Add_NoAvailableSeats_ExceptionThrown()
	{
		//Arrange
		for (var i = 0; i < EventTotalSeats; i++)
		{
			await _service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId1), CancellationToken.None);
		}

		//Act
		Func<Task> act = () => _service.Add(new BookingToAddDto(_bookingId, _eventId1), CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<NoAvailableSeatsException>()
			.WithMessage("No available seats for this event.");
		Func<Task> act2 = () => _service.GetById(_bookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
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
			.Select(async _ =>
			{
				try
				{
					var bookingId = Guid.NewGuid();
					bookingIdsList.Add(bookingId);
					await _service.Add(new BookingToAddDto(bookingId, _eventId1), CancellationToken.None);
					Interlocked.Increment(ref successCount);
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			}).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		successCount.Should().Be(EventTotalSeats);
		exceptionsCount.Should().Be(0);
		(await _eventRepository.Find(_eventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
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
			.Select(async _ =>
			{
				try
				{
					await _service.Add(new BookingToAddDto(Guid.NewGuid(), _eventId1), CancellationToken.None);
					Interlocked.Increment(ref successCount);
				}
				catch (NoAvailableSeatsException)
				{
					Interlocked.Increment(ref exceptionsCount);
				}
			}).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		successCount.Should().Be(EventTotalSeats);
		exceptionsCount.Should().Be(totalRequests - EventTotalSeats);
		(await _eventRepository.Find(_eventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(_bookingId, _eventId1);
		await _service.Add(dto, CancellationToken.None);

		//Act
		var result = await _service.GetById(_bookingId, CancellationToken.None);

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
	public async Task GetById_NonExistentBooking_Failed()
	{
		//Act
		Func<Task> act = () => _service.GetById(_bookingId, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{_bookingId.ToString()}] не найдена.");
	}
}