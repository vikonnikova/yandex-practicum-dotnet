using Events.Domain;

namespace Events.UnitTests.Domain;

public class EventPeriodTests
{
	/// <summary>
	/// Проверяет создание периода события.
	/// </summary>
	[Fact]
	public void Create_WhenValidData_ShouldWorkCorrectly()
	{
		var startAt = DateTime.UtcNow;
		var endAt = startAt.AddDays(2);

		var eventPeriod = EventPeriod.Create(startAt, endAt);

		Assert.Equal(startAt, eventPeriod.StartAt);
		Assert.Equal(endAt, eventPeriod.EndAt);
	}

	/// <summary>
	/// Проверяет, что выбрасывается исключение, если не передать дату начала события или передать дефолтное значение.
	/// </summary>
	[Fact]
	public void Create_WhenStartAtIsDefault_ShouldThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => EventPeriod.Create(default, DateTime.UtcNow));
	}

	/// <summary>
	/// Проверяет, что выбрасывается исключение, если не передать дату окончания события или передать дефолтное значение.
	/// </summary>
	[Fact]
	public void Create_WhenEndAtIsDefault_ShouldThrowArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => EventPeriod.Create(DateTime.UtcNow, default));
	}

	/// <summary>
	/// Проверяет, что выбрасывается исключение, если даты начала и окончания равны.
	/// </summary>
	[Fact]
	public void Create_WhenEndAtEqualStartAt_ShouldThrowArgumentException()
	{
		var date = DateTime.UtcNow;
		Assert.Throws<ArgumentException>(() => EventPeriod.Create(date, date));
	}

	/// <summary>
	/// Проверяет, что выбрасывается исключение, если дата окончания события меньше даты начала.
	/// </summary>
	[Fact]
	public void Create_WhenEndAtLessThanStartAt_ShouldThrowArgumentException()
	{
		var startAt = DateTime.UtcNow;
		Assert.Throws<ArgumentException>(() => EventPeriod.Create(startAt, startAt.AddDays(-1)));
	}
}