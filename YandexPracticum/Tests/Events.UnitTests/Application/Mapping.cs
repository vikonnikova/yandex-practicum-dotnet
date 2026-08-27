namespace Events.UnitTests.Application;

public class Mapping
{
    /*#region Event

	/// <summary>
	/// Проверяет маппинг доменной сущности События в dto слоя Application.
	/// </summary>
	[Fact]
	public void ToDto_WhenEventIsValid_ShouldWorkCorrectly()
	{
		var startAt = DateTime.UtcNow;
		var endAt = startAt.AddDays(2);

		var @event = Event.Create(Guid.NewGuid(), "Новый год", "Дед мороз и снегурочка",
			EventPeriod.Create(startAt, endAt), 90);

		var eventDto = @event.ToDto();

		Assert.Multiple(() =>
		{
			Assert.Equal(eventDto.Id, @event.Id);
			Assert.Equal(eventDto.Title, @event.Title);
			Assert.Equal(eventDto.Description, @event.Description);
			Assert.Equal(eventDto.StartAt, @event.Period.StartAt);
			Assert.Equal(eventDto.EndAt, @event.Period.EndAt);
			Assert.Equal(eventDto.TotalSeats, @event.TotalSeats);
			Assert.Equal(eventDto.AvailableSeats, @event.AvailableSeats);
		});
	}

	#endregion

	#region Booking

	/// <summary>
	/// Проверяет маппинг доменной сущности Бронирования в dto слоя Application.
	/// </summary>
	[Fact]
	public void ToDto_WhenBookingIsValid_ShouldWorkCorrectly()
	{
		var booking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow);

		var bookingDto = booking.ToDto();

		Assert.Multiple(() =>
		{
			Assert.Equal(booking.Id, bookingDto.BookingId);
			Assert.Equal(booking.EventId, bookingDto.EventId);
			Assert.Equal(booking.Status, bookingDto.Status);
		});
	}

	#endregion*/
}