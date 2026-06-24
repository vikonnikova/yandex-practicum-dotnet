using System.Collections.Concurrent;
using Events.Application.Exceptions;
using Events.Application.UseCases;
using Events.Application.UseCases.Dto;
using Events.Domain;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			returnedResult = await service.Add(dto, CancellationToken.None);
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			returnedResult.Should().NotBeNull();
			returnedResult.BookingId.Should().Be(BookingId);
			returnedResult.EventId.Should().Be(EventId1);
			returnedResult.Status.Should().Be(BookingStatus.Pending);

			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			var result = await service.GetById(returnedResult.BookingId, CancellationToken.None);

			result.Should().NotBeNull();
			result.BookingId.Should().Be(BookingId);
			result.EventId.Should().Be(EventId1);
			result.Status.Should().Be(BookingStatus.Pending);

			var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await eventService.GetById(EventId1, CancellationToken.None))
				.AvailableSeats.Should().Be(EventTotalSeats - 1);
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
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			Func<Task> act = () => service.Add(dto, CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{eventId.ToString()}] не найдена.");
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
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
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			await service.Remove(EventId2, CancellationToken.None);
		}

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			Func<Task> act = () => service.Add(new BookingToAddDto(BookingId, EventId2), CancellationToken.None);
			await act.Should().ThrowAsync<EntityNotFoundException>()
				.WithMessage($"Сущность [Событие] с идентификатором [{EventId2.ToString()}] не найдена.");
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
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
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			for (var i = 0; i < EventTotalSeats; i++)
			{
				await service.Add(new BookingToAddDto(Guid.NewGuid(), EventId1), CancellationToken.None);
			}
		}

		//Act
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
			Func<Task> act = () => service.Add(new BookingToAddDto(BookingId, EventId1), CancellationToken.None);
			await act.Should().ThrowAsync<NoAvailableSeatsException>()
				.WithMessage("No available seats for this event.");
		}

		//Assert
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
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
					using (var scope = ServiceProvider.CreateScope())
					{
						var bookingId = Guid.NewGuid();
						bookingIdsList.Add(bookingId);

						var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
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

		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await service.GetById(EventId1, CancellationToken.None))!.AvailableSeats.Should().Be(0);
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
					using (var scope = ServiceProvider.CreateScope())
					{
						var service = scope.ServiceProvider.GetRequiredService<IBookingService>();
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
		using (var scope = ServiceProvider.CreateScope())
		{
			var service = scope.ServiceProvider.GetRequiredService<IEventService>();
			(await service.GetById(EventId1, CancellationToken.None)).AvailableSeats.Should().Be(0);
		}
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_Success()
	{
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

		//Act
		var result = await service.GetById(EventId2BookingId, CancellationToken.None);

		//Assert
		result.Should().NotBeNull();
		result.BookingId.Should().Be(EventId2BookingId);
		result.EventId.Should().Be(EventId2);
		result.Status.Should().Be(BookingStatus.Pending);
	}

	/// <summary>
	/// Проверяет получение несуществующей брони.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentBooking_Failed()
	{
		using var scope = ServiceProvider.CreateScope();

		//Arrange
		var service = scope.ServiceProvider.GetRequiredService<IBookingService>();

		//Act
		Func<Task> act = () => service.GetById(BookingId, CancellationToken.None);

		//Assert
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Бронь] с идентификатором [{BookingId.ToString()}] не найдена.");
	}
}