using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.IntegrationTests.Infrastructure;

namespace Events.IntegrationTests.Api;

[Collection("API Application Collection")]
public class BookingApiTests(ApiWebApplicationFactory factory) : BaseApiTest(factory)
{
	/*
	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_200Returned()
	{
		//Arrange
		var eventId = await CreateEvent();
		var bookingId = await CreateBooking(eventId);

		//Act
		var response = await Client.GetAsync($"/bookings/{bookingId}");

		//Assert
		var responseData = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(bookingId, responseData.BookingId);
		Assert.Equal(eventId, responseData.EventId);
		Assert.Equal(BookingStatus.Pending, responseData.Status);
	}
	
	/// <summary>
	/// Проверяет получение несуществующего бронирования.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentBooking_404Returned()
	{
		//Arrange
		await CreateEvents();

		//Act
		var response = await Client.GetAsync($"/bookings/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
	*/
}