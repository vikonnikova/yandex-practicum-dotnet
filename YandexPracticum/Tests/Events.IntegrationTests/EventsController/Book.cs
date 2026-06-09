using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.IntegrationTests.Api;

namespace Events.IntegrationTests.EventsController;

public class BookTests : BaseApiTest
{
	/// <summary>
	/// Проверяет создание заявки на бронирование.
	/// </summary>
	[Fact]
	public async Task Book_ValidData_202Returned()
	{
		//Arrange
		var eventId = await CreateEvent();
		
		//Act
		var response = await Client.PostAsync($"/events/{eventId}/book", null);

		//Assert
		var bookingId = await response.Content.ReadFromJsonAsync<Guid>();
		Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
		Assert.Equal(new Uri($"/Bookings/{bookingId}", UriKind.Relative), response.Headers.Location);

		var createdBooking = (await Client.GetFromJsonAsync<BookingResponse>($"/bookings/{bookingId}"))!;
		//Assert.Equal(bookingId, createdBooking.BookingId); TODO
		Assert.Equal(eventId, createdBooking.EventId);
		Assert.Equal(BookingStatus.Pending, createdBooking.Status);
	}
	
	/// <summary>
	/// Проверяет создание заявки на бронирование на несуществующее событие.
	/// </summary>
	[Fact]
	public async Task Book_NonExistentEvent_404Returned()
	{
		await CreateEvents();
		
		//Act
		var response = await Client.PostAsync($"/events/{Guid.NewGuid()}/book", null);

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}