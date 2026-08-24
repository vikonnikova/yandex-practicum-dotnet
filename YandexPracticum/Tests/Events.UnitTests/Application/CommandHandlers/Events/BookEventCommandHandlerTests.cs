using System.Collections.Concurrent;
using Events.Application.Contracts.Commands.Events;
using Events.Application.Contracts.Queries.Events;
using Events.Application.Exceptions;
using Events.Application.QueryHandlers.Events;
using Events.Application.UseCases.Events;
using Events.Domain;
using Events.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Events.UnitTests.Application.CommandHandlers.Events;

public class BookEventCommandHandlerTests : BaseUnitTest
{
	/// <summary>
	/// Проверяет создание брони.
	/// </summary>
	[Fact]
	public async Task Add_WhenValidData_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();
		var command = new BookEventCommand(EventId);

		//Act
		var result = await handler.Handle(command, CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.Is<Booking>(x => x.EventId == EventId)),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);

		result.Should().NotBeNull();
		result.EventId.Should().Be(EventId);
		result.Status.Should().Be(BookingStatus.Pending);
	}

	/// <summary>
	/// Проверяет создание брони на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Add_WhenEventDoesNotExist_ShouldThrowEntityNotFoundException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();
		var nonExistentEventId = Guid.NewGuid();
		var command = new BookEventCommand(nonExistentEventId);

		//Act
		Func<Task> act = () => handler.Handle(command, CancellationToken.None);
		await act.Should().ThrowAsync<EntityNotFoundException>()
			.WithMessage($"Сущность [Событие] с идентификатором [{nonExistentEventId.ToString()}] не найдена.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == nonExistentEventId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Booking>()),
			Times.Never);

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	/// <summary>
	/// Проверяет создание брони на прошедшее событие.
	/// </summary>
	[Fact]
	public async Task Add_WhenPastEvent_ShouldThrowPastEventBookingException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();
		var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>() as FakeTimeProvider;
		timeProvider?.SetUtcNow(DateTime.UtcNow);

		//Act
		Func<Task> act = () => handler.Handle(new BookEventCommand(EventId), CancellationToken.None);
		await act.Should().ThrowAsync<PastEventBookingException>()
			.WithMessage("Попытка забронировать прошедшее событие.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Booking>()),
			Times.Never);

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	/// <summary>
	/// Проверяет создание брони при достижении лимита пользователем.
	/// </summary>
	[Fact]
	public async Task Add_WhenBookingLimitReached_ShouldThrowBookingLimitReachingException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		BookingRepositoryMock.Setup(repo => repo.CountBy(EventId, UserId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(10);
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();

		//Act
		Func<Task> act = () => handler.Handle(new BookEventCommand(EventId), CancellationToken.None);
		await act.Should().ThrowAsync<BookingLimitReachingException>()
			.WithMessage("Достигнут лимит [10] бронирования у события.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Booking>()),
			Times.Never);

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Never);
	}

	/// <summary>
	/// Проверяет, что лимиты разных пользователей не влияют друг на друга.
	/// </summary>
	[Fact]
	public async Task Add_WhenBookingLimitReachedForOtherUser_ShouldWorkCorrectly()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		BookingRepositoryMock.Setup(repo => repo.CountBy(EventId, Guid.NewGuid(), It.IsAny<CancellationToken>()))
			.ReturnsAsync(10);
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();

		//Act
		var result = await handler.Handle(new BookEventCommand(EventId), CancellationToken.None);

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.CountBy(It.Is<Guid>(x => x == EventId), It.Is<Guid>(x => x == UserId), CancellationToken.None),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Booking>()),
			Times.Once);

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Once);
	}

	/// <summary>
	/// Проверяет создание брони на недоступное количество мест.
	/// </summary>
	[Fact]
	public async Task Add_WhenNoAvailableSeats_ShouldThrowNoAvailableSeatsException()
	{
		//Arrange
		using var scope = ServiceProvider.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();

		for (var i = 0; i < EventTotalSeats; i++)
		{
			await handler.Handle(new BookEventCommand(EventId), CancellationToken.None);
		}

		//Act
		Func<Task> act = () => handler.Handle(new BookEventCommand(EventId), CancellationToken.None);
		await act.Should().ThrowAsync<NoAvailableSeatsException>()
			.WithMessage("Нет доступных мест для бронирования на запрашиваемое событие.");

		//Assert
		EventRepositoryMock.Verify(
			repo => repo.Find(It.Is<Guid>(x => x == EventId), CancellationToken.None),
			Times.Exactly(EventTotalSeats + 1));

		BookingRepositoryMock.Verify(
			repo => repo.Add(It.IsAny<Booking>()),
			Times.Exactly(EventTotalSeats));

		BookingRepositoryMock.Verify(
			repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()),
			Times.Exactly(EventTotalSeats));
	}

	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события.
	/// </summary>
	[Fact]
	public async Task Add_WhenMultipleBookingsForOneEvent_ShouldWorkCorrectly()
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

						var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();
						await handler.Handle(new BookEventCommand(EventId), CancellationToken.None);

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
			var handler = scope.ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();
			(await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None)).AvailableSeats.Should()
				.Be(0);
		}
	}

	/// <summary>
	/// Проверяет создание нескольких броней с уникальными идентификаторами для одного события при овербукинге.
	/// </summary>
	[Fact]
	public async Task Add_WhenMultipleBookingsForOneEvent_Overbooking_ShouldWorkCorrectly()
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
						var handler = scope.ServiceProvider.GetRequiredService<BookEventCommandHandler>();
						await handler.Handle(new BookEventCommand(EventId), CancellationToken.None);

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
			var handler = scope.ServiceProvider.GetRequiredService<GetEventByIdQueryHandler>();
			(await handler.Handle(new GetEventByIdQuery(EventId), CancellationToken.None)).AvailableSeats.Should()
				.Be(0);
		}
	}
}