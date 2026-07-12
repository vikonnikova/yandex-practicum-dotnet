using Events.Domain;
using Events.Infrastructure;
using Events.IntegrationTests.Infrastructure;
using Events.IntegrationTests.Repositories.Base;
using FluentAssertions;

namespace Events.IntegrationTests.Repositories;

[Collection("Database collection")]
public class BookingRepositoryTests(DbFixture dbFixture) : BaseRepositoryTest(dbFixture)
{
	/// <summary>
	/// Проверяет поиск брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenValidData_ShouldReturnBooking()
	{
		// Arrange
		var booking = Booking.Create(BookingId, EventId, DateTime.UtcNow);

		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			context.Bookings.Add(booking);
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var result = await new BookingRepository(context).Find(BookingId, CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Id.Should().Be(BookingId);
			result.EventId.Should().Be(EventId);
		}
	}

	/// <summary>
	/// Проверяет поиск несуществующей брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task Find_WhenBookingDoesNotExist_ShouldReturnNull()
	{
		// Arrange
		await using var context = DbFixture.CreateContext();

		// Act
		var result = await new BookingRepository(context).Find(Guid.NewGuid(), CancellationToken.None);

		// Assert
		result.Should().BeNull();
	}

	/// <summary>
	/// Проверяет получение необработанных броней.
	/// </summary>
	[Fact]
	public async Task GetPending_WhenDifferentStatuses_ShouldReturnOnlyPendingBookings()
	{
		// Arrange
		var pendingBooking1 = Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow);
		var pendingBooking2 = Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow);
		var confirmedBooking = Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow);
		var rejectedBooking = Booking.Create(Guid.NewGuid(), EventId, DateTime.UtcNow);

		confirmedBooking.Confirm(DateTime.UtcNow);
		rejectedBooking.Reject(DateTime.UtcNow);

		await using (var context = DbFixture.CreateContext())
		{
			context.Events.Add(CreateEvent());
			context.Bookings.AddRange(pendingBooking1, pendingBooking2, confirmedBooking, rejectedBooking);
			await context.SaveChangesAsync();
		}

		await using (var context = DbFixture.CreateContext())
		{
			// Act
			var result = await new BookingRepository(context).GetPending(CancellationToken.None);

			// Assert
			result.Should().NotBeNull();
			result.Should().Contain([pendingBooking1.Id, pendingBooking2.Id]);
			result.Should().NotContain([confirmedBooking.Id, rejectedBooking.Id]);
		}
	}

	/// <summary>
	/// Проверяет сохранение брони.
	/// </summary>
	[Fact]
	public async Task Add_And_SaveChangesAsync_WhenValidData_ShouldSaveCorrectly()
	{
		await using (var context = DbFixture.CreateContext())
		{
			// Arrange
			context.Events.Add(CreateEvent());
			await context.SaveChangesAsync();

			// Act
			var repository = new BookingRepository(context);
			repository.Add(Booking.Create(BookingId, EventId, DateTime.UtcNow));
			await repository.SaveChangesAsync(CancellationToken.None);
		}

		// Assert
		await using (var context = DbFixture.CreateContext())
		{
			var result = await context.Bookings.FindAsync(BookingId);

			result.Should().NotBeNull();
			result.Id.Should().Be(BookingId);
			result.EventId.Should().Be(EventId);
		}
	}
}