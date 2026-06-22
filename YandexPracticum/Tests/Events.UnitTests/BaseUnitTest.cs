using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
	protected readonly DateTime Now = DateTime.UtcNow;
	protected static readonly Guid EventId1 = Guid.NewGuid();
	protected static readonly Guid EventId2 = Guid.NewGuid();
	protected static readonly Guid EventId3 = Guid.NewGuid();
	protected static readonly Guid BookingId = Guid.NewGuid();
	protected const int EventTotalSeats = 10;

	protected AppDbContext CreateContext(DbContextOptions<AppDbContext> options) => new(options);

	protected void SeedDatabase(DbContextOptions<AppDbContext> options)
	{
		using var context = CreateContext(options);

		context.Events.AddRange(
			Event.Create(EventId1, "День рождения", "Дед Мороз и снегурочка",
				EventPeriod.Create(Now, Now.AddDays(7)), EventTotalSeats),
			Event.Create(EventId2, "Пасха", "Красим яйца, печем куличи",
				EventPeriod.Create(Now.AddHours(-12), Now.AddHours(-10)), 20),
			Event.Create(EventId3, "Рождество", "описание рождества, подарки, игрушки",
				EventPeriod.Create(Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2)), 100),
			Event.Create(Guid.NewGuid(), "23 февраля", "День защитника отечества",
				EventPeriod.Create(Now.AddDays(-7), Now.AddDays(-6)), 5),
			Event.Create(Guid.NewGuid(), "День победы", "Парад и салют",
				EventPeriod.Create(Now, Now.AddHours(14)), 7)
		);

		context.Bookings.AddRange(
			Booking.Create(Guid.NewGuid(), EventId2, DateTime.UtcNow),
			Booking.Create(Guid.NewGuid(), EventId3, DateTime.UtcNow)
		);

		context.SaveChanges();
	}

	public abstract void Dispose();
}