using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Domain;
using Events.Infrastructure;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
	protected readonly DateTime Now = DateTime.UtcNow;
	protected static readonly Guid EventId1 = Guid.NewGuid();
	protected static readonly Guid EventId2 = Guid.NewGuid();
	protected static readonly Guid EventId3 = Guid.NewGuid();
	protected static readonly Guid BookingId = Guid.NewGuid();
	protected static readonly Guid EventId2BookingId = Guid.NewGuid();
	protected const int EventTotalSeats = 10;

	protected readonly IServiceProvider ServiceProvider;

	protected BaseUnitTest()
	{
		var dbName = Guid.NewGuid().ToString();

		var services = new ServiceCollection();
		services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
		services.AddScoped<IEventRepository, EventRepository>();
		services.AddScoped<IBookingRepository, BookingRepository>();
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		ServiceProvider = services.BuildServiceProvider();

		SeedDatabase();
	}

	private void SeedDatabase()
	{
		using var scope = ServiceProvider.CreateScope();
		var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
			Booking.Create(EventId2BookingId, EventId2, DateTime.UtcNow),
			Booking.Create(Guid.NewGuid(), EventId3, DateTime.UtcNow)
		);

		context.SaveChanges();
	}

	public void Dispose()
	{
		if (ServiceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}