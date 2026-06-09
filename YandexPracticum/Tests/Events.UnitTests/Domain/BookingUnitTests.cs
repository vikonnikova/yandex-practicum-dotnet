using Events.Domain;
using FluentAssertions;

namespace Events.UnitTests.Domain;

public class BookingUnitTests
{
	/// <summary>
	/// Проверяет создание бронирования.
	/// </summary>
	[Fact]
	public void Create_Success()
	{
		var bookingId = Guid.NewGuid();
		var eventId = Guid.NewGuid();
		var now = DateTime.UtcNow;

		var booking = Booking.Create(bookingId, eventId, now);
		
		booking.Id.Should().Be(bookingId);
		booking.EventId.Should().Be(eventId);
		booking.CreatedAt.Should().Be(now);
		booking.Status.Should().Be(BookingStatus.Pending);
		booking.ProcessedAt.Should().BeNull();
	}

	/// <summary>
	/// Проверяет подтверждение бронирования.
	/// </summary>
	[Fact]
	public void Confirm_Success()
	{
		var bookingId = Guid.NewGuid();
		var eventId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var confirmedAt = DateTime.UtcNow.AddDays(5).AddHours(4).AddMinutes(8);
		var booking = Booking.Create(bookingId, eventId, now);
		
		booking.Confirm(confirmedAt);
		
		booking.Id.Should().Be(bookingId);
		booking.EventId.Should().Be(eventId);
		booking.CreatedAt.Should().Be(now);
		booking.Status.Should().Be(BookingStatus.Confirmed);
		booking.ProcessedAt.Should().Be(confirmedAt);
	}
	
	/// <summary>
	/// Проверяет отклонение бронирования.
	/// </summary>
	[Fact]
	public void Reject_Success()
	{
		var bookingId = Guid.NewGuid();
		var eventId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		var rejectedAt = DateTime.UtcNow.AddDays(1).AddHours(6).AddMinutes(20);
		var booking = Booking.Create(bookingId, eventId, now);
		
		booking.Confirm(rejectedAt);
		
		booking.Id.Should().Be(bookingId);
		booking.EventId.Should().Be(eventId);
		booking.CreatedAt.Should().Be(now);
		booking.Status.Should().Be(BookingStatus.Rejected);
		booking.ProcessedAt.Should().Be(rejectedAt);
	}
}