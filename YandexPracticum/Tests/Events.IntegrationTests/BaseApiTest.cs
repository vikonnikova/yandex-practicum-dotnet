using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.Infrastructure.DataAccess;
using Microsoft.Extensions.DependencyInjection;

namespace Events.IntegrationTests;

[Collection("Database Integration Tests")]
public abstract class BaseApiTest : IClassFixture<ApiWebApplicationFactory>
{
	protected readonly ApiWebApplicationFactory Factory;
	protected readonly HttpClient Client;

	protected BaseApiTest(ApiWebApplicationFactory factory)
	{
		Factory = factory;
		Client = factory.CreateClient();
		EnsureCreated();
		CleanupDatabase();
	}

	protected async Task<Guid> CreateEvent()
	{
		var response = await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());
		var responseData = (await response.Content.ReadFromJsonAsync<EventResponse>())!;

		return responseData.Id;
	}

	protected async Task CreateEvents()
	{
		foreach (var @event in TestData.CreateTestEvents())
		{
			await Client.PostAsJsonAsync("/events", @event);
		}
	}

	protected async Task<Guid> CreateBooking(Guid eventId)
	{
		var response = await Client.PostAsync($"/events/{eventId}/book", null);
		var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;

		return booking.BookingId;
	}

	protected async Task CreateBookings(Guid eventId, int bookCount)
	{
		var tasks = Enumerable.Range(0, bookCount)
			.Select(_ => Task.Run(async () => { await Client.PostAsync($"/events/{eventId}/book", null); })).ToArray();

		await Task.WhenAll(tasks);
	}
	
	public void EnsureCreated()
	{
		using var scope = Factory.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		dbContext.Database.EnsureCreated();
	}
	
	public void CleanupDatabase()
	{
		using var scope = Factory.Services.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		
		dbContext.Bookings.RemoveRange(dbContext.Bookings);
		dbContext.Events.RemoveRange(dbContext.Events);
		dbContext.SaveChanges();
	}
}