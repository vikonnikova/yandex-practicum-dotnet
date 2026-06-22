using System.Collections.Concurrent;
using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using Events.Domain;
using Events.Infrastructure;
using Events.Infrastructure.DataAccess;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Events.UnitTests.Application;

public class BookingServiceUnitTests : BaseUnitTest
{
	private readonly AppDbContext _context;
	private readonly EventRepository _eventRepository;
	private readonly BookingService _service;

	public BookingServiceUnitTests()
	{
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
			.Options;

		_context = new AppDbContext(options);
		_eventRepository = new EventRepository(_context);
		_service = new BookingService(new BookingRepository(_context), _eventRepository);

		SeedDatabase(options);
	}

	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(BookingId, EventId1);

		//Act
		var result = await _service.Add(dto, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(BookingId);
		result.EventId.Should().Be(EventId1);
		result.Status.Should().Be(BookingStatus.Pending);
		(await _service.GetById(BookingId, CancellationToken.None)).Should().BeEquivalentTo(dto);
		(await _eventRepository.Find(EventId1, CancellationToken.None))!.AvailableSeats.Should()
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
		var dto = new BookingToAddDto(BookingId, eventId);

		//Act
		Func<Task> act = () => _service.Add(dto, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		Func<Task> act2 = () => _service.GetById(BookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Add_ForDeletedEvent_Failed()
	{
		//Arrange
		await _service.Add(new BookingToAddDto(Guid.NewGuid(), EventId2), CancellationToken.None);
		await new EventService(_eventRepository).Remove(EventId2, CancellationToken.None);

		//Act
		Func<Task> act = () =>
			_service.Add(new BookingToAddDto(BookingId, EventId2), CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{EventId2.ToString()}] не найдена.");
		Func<Task> act2 = () => _service.GetById(BookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
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
			await _service.Add(new BookingToAddDto(Guid.NewGuid(), EventId1), CancellationToken.None);
		}

		//Act
		Func<Task> act = () =>
			_service.Add(new BookingToAddDto(BookingId, EventId1), CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<NoAvailableSeatsException>()
			.WithMessage("No available seats for this event.");
		Func<Task> act2 = () => _service.GetById(BookingId, CancellationToken.None);
		await act2.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
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
					await _service.Add(new BookingToAddDto(bookingId, EventId1), CancellationToken.None);
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
		(await _eventRepository.Find(EventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
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
					await _service.Add(new BookingToAddDto(Guid.NewGuid(), EventId1), CancellationToken.None);
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
		(await _eventRepository.Find(EventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(BookingId, EventId1);
		await _service.Add(dto, CancellationToken.None);

		//Act
		var result = await _service.GetById(BookingId, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(BookingId);
		result.EventId.Should().Be(EventId1);
		result.Status.Should().Be(BookingStatus.Pending);
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentBooking_Failed()
	{
		//Act
		Func<Task> act = () => _service.GetById(BookingId, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
	}

	public override void Dispose()
	{
		_context.Dispose();
	}
}