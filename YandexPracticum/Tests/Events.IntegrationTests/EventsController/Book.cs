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
		var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
		Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
		Assert.Equal(new Uri($"/Bookings/{booking.BookingId}", UriKind.Relative), response.Headers.Location);
		Assert.Equal(eventId, booking.EventId);
		Assert.Equal(BookingStatus.Pending, booking.Status);

		var createdBooking = (await Client.GetFromJsonAsync<BookingResponse>($"/bookings/{booking.BookingId}"))!;
		Assert.Equal(booking.BookingId, createdBooking.BookingId);
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