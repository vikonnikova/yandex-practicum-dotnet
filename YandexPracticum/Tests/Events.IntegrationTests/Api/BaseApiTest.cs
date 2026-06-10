using System.Net.Http.Json;
using Events.Api.Contracts;

namespace Events.IntegrationTests.Api;

public class BaseApiTest : /*IClassFixture<ApiWebApplicationFactory>,*/ IDisposable
{
	protected readonly ApiWebApplicationFactory Factory;
	protected readonly HttpClient Client;

	protected BaseApiTest()
	{
		Factory = new ApiWebApplicationFactory();
		Client = Factory.CreateClient();
	}

	public void Dispose()
	{
		Client.Dispose();
		Factory.Dispose();
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
}