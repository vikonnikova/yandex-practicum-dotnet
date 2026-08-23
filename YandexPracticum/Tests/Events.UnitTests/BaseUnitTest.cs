using System.Security.Cryptography;
using System.Text;
using Events.Application;
using Events.Application.Interfaces;
using Events.Application.QueryHandlers.Bookings;
using Events.Application.QueryHandlers.Events;
using Events.Application.UseCases.Auth;
using Events.Application.UseCases.Events;
using Events.Domain;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Events.UnitTests;

public abstract class BaseUnitTest : IDisposable
{
	protected readonly Guid EventId = Guid.NewGuid();
	protected readonly Guid BookingId = Guid.NewGuid();
	protected readonly Guid UserId = Guid.NewGuid();
	protected const string UserLogin = "Ivan_123";
	protected const string UserPassword = "qwerty1234";
	protected const string UserPasswordHash = "17F80754644D33AC685B0842A402229ADBB43FC9312F7BDF36BA24237A1F1FFB";
	protected const string EventTitle = "Новый год";
	protected const string EventDescription = "Дед Мороз и снегурочка";
	protected readonly DateTime EventStartAt = new(2022, 01, 01, 00, 00, 00, DateTimeKind.Utc);
	protected readonly DateTime EventEndAt = new(2022, 01, 10, 23, 59, 59, DateTimeKind.Utc);
	protected const int EventTotalSeats = 7;
	protected const int Page = 3;
	protected const int PageSize = 15;

	protected readonly Mock<IJwtProvider> JwtProviderMock = new();
	protected readonly Mock<IPasswordHasher> PasswordHasherMock = new();
	protected readonly Mock<IUserRepository> UserRepositoryMock = new();
	protected readonly Mock<IEventRepository> EventRepositoryMock = new();
	protected readonly Mock<IBookingRepository> BookingRepositoryMock = new();

	protected readonly IServiceProvider ServiceProvider;

	protected BaseUnitTest()
	{
		var services = new ServiceCollection();
		ConfigureRepositories(services);

		services.AddScoped<RegisterUserCommandHandler>();
		services.AddScoped<LoginCommandHandler>();

		services.AddScoped<GetEventsByQueryHandler>();
		services.AddScoped<GetEventByIdQueryHandler>();
		services.AddScoped<GetBookingByIdQueryHandler>();

		services.AddScoped<AddEventCommandHandler>();
		services.AddScoped<UpdateEventCommandHandler>();
		services.AddScoped<RemoveEventCommandHandler>();
		services.AddScoped<BookEventCommandHandler>();

		ServiceProvider = services.BuildServiceProvider();
	}

	private void ConfigureRepositories(IServiceCollection services)
	{
		var user = User.Create(UserId, UserLogin, UserPasswordHash, UserRole.User);
		var @event = Event.Create(EventId, EventTitle, EventDescription,
			EventPeriod.Create(EventStartAt, EventEndAt), EventTotalSeats);
		var booking = Booking.Create(BookingId, EventId, UserId, DateTime.UtcNow);
		
		JwtProviderMock.Setup(provider => provider.GenerateToken(user))
			.Returns("jwt_token");

		PasswordHasherMock.Setup(hasher => hasher.Hash(UserPassword))
			.Returns(UserPasswordHash);
		PasswordHasherMock.Setup(hasher => hasher.Verify(UserPassword, UserPasswordHash))
			.Returns(true);

		UserRepositoryMock.Setup(repo => repo.FindByLogin(UserLogin, It.IsAny<CancellationToken>()))
			.ReturnsAsync(user);
		UserRepositoryMock.Setup(repo => repo.ExistsByLogin(UserLogin, It.IsAny<CancellationToken>()))
			.ReturnsAsync(false);
		UserRepositoryMock.Setup(repo => repo.Add(user));
		UserRepositoryMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

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

		services.AddSingleton(JwtProviderMock.Object);
		services.AddSingleton(PasswordHasherMock.Object);
		services.AddSingleton(UserRepositoryMock.Object);
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