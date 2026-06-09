using Events.Domain;

namespace Events.UnitTests.Domain;

public class EventUnitTests
{
	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public void Create_Success()
	{
		var eventId = Guid.NewGuid();
		var startAt = DateTime.UtcNow;
		var endAt = startAt.AddDays(2);

		var @event = Event.Create(eventId, "Title", "Description", EventPeriod.Create(startAt, endAt));

		Assert.Multiple(() =>
		{
			Assert.Equal(eventId, @event.Id);
			Assert.Equal("Title", @event.Title);
			Assert.Equal("Description", @event.Description);
			Assert.Equal(startAt, @event.Period.StartAt);
			Assert.Equal(endAt, @event.Period.EndAt);
		});
	}

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public void Update_Success()
	{
		var eventId = Guid.NewGuid();
		var utcNow = DateTime.UtcNow;
		var startAt = utcNow.AddDays(3).AddHours(4);
		var endAt = startAt.AddHours(5);
		var @event = Event.Create(eventId, "Title", "Description", EventPeriod.Create(utcNow, utcNow.AddDays(2)));

		@event.Update("Наименование", "Описание", EventPeriod.Create(startAt, endAt));

		Assert.Multiple(() =>
		{
			Assert.Equal(eventId, @event.Id);
			Assert.Equal("Наименование", @event.Title);
			Assert.Equal("Описание", @event.Description);
			Assert.Equal(startAt, @event.Period.StartAt);
			Assert.Equal(endAt, @event.Period.EndAt);
		});
	}
}