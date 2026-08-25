using Events.Domain;

namespace Events.UnitTests.Domain;

public class EventTests
{
    /// <summary>
    /// Проверяет создание события.
    /// </summary>
    [Fact]
    public void Create_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(2);

        var @event = Event.Create(eventId, "Новый год", "Дед мороз и снегурочка", EventPeriod.Create(startAt, endAt),
            90);

        Assert.Multiple(() =>
        {
            Assert.Equal(eventId, @event.Id);
            Assert.Equal("Новый год", @event.Title);
            Assert.Equal("Дед мороз и снегурочка", @event.Description);
            Assert.Equal(startAt, @event.Period.StartAt);
            Assert.Equal(endAt, @event.Period.EndAt);
            Assert.Equal(90, @event.TotalSeats);
            Assert.Equal(90, @event.AvailableSeats);
        });
    }

    /// <summary>
    /// Проверяет, что выбрасывается исключение, если общее количество мест меньше либо равно нуля.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenInvalidTotalSeats_ShouldThrowArgumentException(int totalSeats)
    {
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(2);

        Assert.Throws<ArgumentException>(() => Event.Create(eventId, "Новый год", "Дед мороз и снегурочка",
            EventPeriod.Create(startAt, endAt), totalSeats));
    }

    /// <summary>
    /// Проверяет обновление события.
    /// </summary>
    [Fact]
    public void Update_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var utcNow = DateTime.UtcNow;
        var startAt = utcNow.AddDays(3).AddHours(4);
        var endAt = startAt.AddHours(5);
        var @event = Event.Create(eventId, "Новый год", "Дед мороз и снегурочка",
            EventPeriod.Create(utcNow, utcNow.AddDays(2)), 90);

        @event.Update("Рождество", "Рождественский сочельник, Богослужения, Святки",
            EventPeriod.Create(startAt, endAt));

        Assert.Multiple(() =>
        {
            Assert.Equal(eventId, @event.Id);
            Assert.Equal("Рождество", @event.Title);
            Assert.Equal("Рождественский сочельник, Богослужения, Святки", @event.Description);
            Assert.Equal(startAt, @event.Period.StartAt);
            Assert.Equal(endAt, @event.Period.EndAt);
            Assert.Equal(90, @event.TotalSeats);
            Assert.Equal(90, @event.AvailableSeats);
        });
    }

    /// <summary>
    /// Проверяет бронирование мест на событии при условии их наличия.
    /// </summary>
    [Fact]
    public void TryReserveSeats_WhenSeatsAreAvailable_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(2);
        var @event = Event.Create(eventId, "Новый год", "Дед мороз и снегурочка",
            EventPeriod.Create(startAt, endAt), 10);

        var requestResult = @event.TryReserveSeats();

        Assert.Multiple(() =>
        {
            Assert.True(requestResult);
            Assert.Equal(eventId, @event.Id);
            Assert.Equal("Новый год", @event.Title);
            Assert.Equal("Дед мороз и снегурочка", @event.Description);
            Assert.Equal(startAt, @event.Period.StartAt);
            Assert.Equal(endAt, @event.Period.EndAt);
            Assert.Equal(10, @event.TotalSeats);
            Assert.Equal(9, @event.AvailableSeats);
        });
    }

    /// <summary>
    /// Проверяет бронирование мест на событии при условии их отсутствия.
    /// </summary>
    [Fact]
    public void TryReserveSeats_WhenSeatsAreNotAvailable_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(2);
        var @event = Event.Create(eventId, "Новый год", "Дед мороз и снегурочка",
            EventPeriod.Create(startAt, endAt), 10);
        @event.TryReserveSeats(10);

        var requestResult = @event.TryReserveSeats();

        Assert.Multiple(() =>
        {
            Assert.False(requestResult);
            Assert.Equal(eventId, @event.Id);
            Assert.Equal("Новый год", @event.Title);
            Assert.Equal("Дед мороз и снегурочка", @event.Description);
            Assert.Equal(startAt, @event.Period.StartAt);
            Assert.Equal(endAt, @event.Period.EndAt);
            Assert.Equal(10, @event.TotalSeats);
            Assert.Equal(0, @event.AvailableSeats);
        });
    }

    /// <summary>
    /// Проверяет освобождение забронированных мест на событии.
    /// </summary>
    [Fact]
    public void ReleaseSeats_WhenValidData_ShouldWorkCorrectly()
    {
        var eventId = Guid.NewGuid();
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(2);
        var @event = Event.Create(eventId, "Новый год", "Дед мороз и снегурочка",
            EventPeriod.Create(startAt, endAt), 10);
        @event.TryReserveSeats(3);

        @event.ReleaseSeats();

        Assert.Multiple(() =>
        {
            Assert.Equal(eventId, @event.Id);
            Assert.Equal("Новый год", @event.Title);
            Assert.Equal("Дед мороз и снегурочка", @event.Description);
            Assert.Equal(startAt, @event.Period.StartAt);
            Assert.Equal(endAt, @event.Period.EndAt);
            Assert.Equal(10, @event.TotalSeats);
            Assert.Equal(8, @event.AvailableSeats);
        });
    }
}