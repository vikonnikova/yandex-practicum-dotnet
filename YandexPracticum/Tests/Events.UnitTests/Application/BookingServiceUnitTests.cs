using System.Collections.Concurrent;
using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using Events.Domain;
using Events.Infrastructure;
using FluentAssertions;

namespace Events.UnitTests.Application;

public class BookingServiceUnitTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_ValidData_Success()
	{
		//Arrange
		var dto = new BookingToAddDto(BookingId, EventId1);
		BookingDto returnedResult;

		//Act
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			returnedResult = await service.Add(dto, CancellationToken.None);
		}

		//Assert
		await using (var context = CreateContext())
		{
			returnedResult.Should().NotBeNull();
			returnedResult.BookingId.Should().Be(BookingId);
			returnedResult.EventId.Should().Be(EventId1);
			returnedResult.Status.Should().Be(BookingStatus.Pending);

			var eventRepository = new EventRepository(context);
			var service = new BookingService(new BookingRepository(context), eventRepository);
			var result = await service.GetById(returnedResult.BookingId, CancellationToken.None);

			result.Should().NotBeNull();
			result.BookingId.Should().Be(BookingId);
			result.EventId.Should().Be(EventId1);
			result.Status.Should().Be(BookingStatus.Pending);

			(await eventRepository.Find(EventId1, CancellationToken.None))!.AvailableSeats.Should()
				.Be(EventTotalSeats - 1);
		}
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
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act = () => service.Add(dto, CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		}

		//Assert
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act2 = () => service.GetById(BookingId, CancellationToken.None);
			await act2.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
		}
	}

	/// <summary>
	/// Проверяет создание брони на удаленное событие.
	/// </summary>
	[Fact]
	public async Task Add_ForDeletedEvent_Failed()
	{
		//Arrange
		await using (var context = CreateContext())
		{
			await new EventService(new EventRepository(context)).Remove(EventId2, CancellationToken.None);
		}

		//Act
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act = () => service.Add(new BookingToAddDto(BookingId, EventId2), CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{EventId2.ToString()}] не найдена.");
		}

		//Assert
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act2 = () => service.GetById(BookingId, CancellationToken.None);
			await act2.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
		}
	}

	/// <summary>
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public async Task Add_NoAvailableSeats_ExceptionThrown()
	{
		//Arrange
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			for (var i = 0; i < EventTotalSeats; i++)
			{
				await service.Add(new BookingToAddDto(Guid.NewGuid(), EventId1), CancellationToken.None);
			}
		}

		//Act
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act = () => service.Add(new BookingToAddDto(BookingId, EventId1), CancellationToken.None);
			await act.Should().ThrowAsync<NoAvailableSeatsException>()
				.WithMessage("No available seats for this event.");
		}


		//Assert
		await using (var context = CreateContext())
		{
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));
			Func<Task> act2 = () => service.GetById(BookingId, CancellationToken.None);
			await act2.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
		}
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
					await using (var context = CreateContext())
					{
						var bookingId = Guid.NewGuid();
						bookingIdsList.Add(bookingId);
						var service = new BookingService(new BookingRepository(context), new EventRepository(context));
						await service.Add(new BookingToAddDto(bookingId, EventId1), CancellationToken.None);
						Interlocked.Increment(ref successCount);
					}
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
		bookingIdsList.Distinct().Should().HaveCount(successCount);

		await using (var context = CreateContext())
		{
			(await new EventRepository(context).Find(EventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
		}
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
					await using (var context = CreateContext())
					{
						var service = new BookingService(new BookingRepository(context), new EventRepository(context));
						await service.Add(new BookingToAddDto(Guid.NewGuid(), EventId1), CancellationToken.None);
						Interlocked.Increment(ref successCount);
					}
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
		await using (var context = CreateContext())
		{
			(await new EventRepository(context).Find(EventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
		}
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		await using (var context = CreateContext())
		{
			//Arrange
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));

			//Act
			var result = await service.GetById(EventId2BookingId, CancellationToken.None);

			//Assert
			result.Should().NotBeNull();
			result.BookingId.Should().Be(EventId2BookingId);
			result.EventId.Should().Be(EventId2);
			result.Status.Should().Be(BookingStatus.Pending);
		}
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentBooking_Failed()
	{
		await using (var context = CreateContext())
		{
			//Arrange
			var service = new BookingService(new BookingRepository(context), new EventRepository(context));

			//Act
			Func<Task> act = () => service.GetById(BookingId, CancellationToken.None);

			//Assert
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
		}
	}
}