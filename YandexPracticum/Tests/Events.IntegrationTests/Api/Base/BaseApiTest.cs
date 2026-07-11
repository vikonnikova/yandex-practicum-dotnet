using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.IntegrationTests.Infrastructure;

namespace Events.IntegrationTests.Api;

public abstract class BaseApiTest(ApiWebApplicationFactory factory)
{
	protected readonly HttpClient Client = factory.CreateClient();

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
}