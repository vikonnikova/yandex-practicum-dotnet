using Events.Application;
using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
	protected readonly Guid EventId = Guid.NewGuid();
	protected readonly Guid BookingId = Guid.NewGuid();
	protected const string EventTitle = "Новый год";
	protected const string EventDescription = "Дед Мороз и снегурочка";
	protected readonly DateTime EventStartAt = new(2022, 01, 01, 00, 00, 00, DateTimeKind.Utc);
	protected readonly DateTime EventEndAt = new(2022, 01, 10, 23, 59, 59, DateTimeKind.Utc);
	protected const int EventTotalSeats = 7;
	protected const int Page = 3;
	protected const int PageSize = 15;

	protected readonly Mock<IEventRepository> EventRepositoryMock = new();
	protected readonly Mock<IBookingRepository> BookingRepositoryMock = new();

	protected readonly IServiceProvider ServiceProvider;

	protected BaseUnitTest()
	{
		var services = new ServiceCollection();
		ConfigureRepositories(services);
		services.AddScoped<IEventService, EventService>();
		services.AddScoped<IBookingService, BookingService>();
		ServiceProvider = services.BuildServiceProvider();
	}

	private void ConfigureRepositories(IServiceCollection services)
	{
		var @event = Event.Create(EventId, EventTitle, EventDescription,
			EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);
		var booking = Booking.Create(BookingId, EventId, DateTime.UtcNow);

		EventRepositoryMock
			.Setup(repo => repo.GetFiltered(3, 15, new Filters(Title: "День", EventStartAt, EventEndAt),
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(new FilteredResult<Event>(TotalItems: 100, Data: [@event]));

		EventRepositoryMock.Setup(repo => repo.Find(EventId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(@event);

		EventRepositoryMock.Setup(repo => repo.Add(@event));

		EventRepositoryMock.Setup(repo => repo.Delete(@event));

		EventRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		BookingRepositoryMock.Setup(repo => repo.Find(BookingId, It.IsAny<CancellationToken>()))
			.ReturnsAsync(booking);

		BookingRepositoryMock.Setup(repo => repo.GetPending(It.IsAny<CancellationToken>()))
			.ReturnsAsync([Guid.NewGuid(), Guid.NewGuid()]);

		BookingRepositoryMock.Setup(repo => repo.Add(booking));

		BookingRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		services.AddSingleton(EventRepositoryMock.Object);
		services.AddSingleton(BookingRepositoryMock.Object);
	}

	public void Dispose()
	{
		if (ServiceProvider is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}
}