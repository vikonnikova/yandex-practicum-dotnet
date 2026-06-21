using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.IntegrationTests.Api;
using FluentAssertions;

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
	/// Проверяет создание заявки на бронирование при овербукинге.
	/// </summary>
	[Fact]
	public async Task Book_Overbooking_409Returned()
	{
		//Arrange
		var eventId = await CreateEvent();
		await CreateBookings(eventId, TestData.TotalSeats);

		//Act
		var response = await Client.PostAsync($"/events/{eventId}/book", null);

		//Assert
		Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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

	/// <summary>
	/// Проверяет параллельное создание заявок на бронирование.
	/// </summary>
	[Fact]
	public async Task Book_MultipleValidData_Success()
	{
		//Arrange
		var totalRequests = TestData.TotalSeats;
		var responses = new ConcurrentBag<HttpResponseMessage>();

		var eventId = await CreateEvent();

		//Act
		var tasks = Enumerable.Range(0, totalRequests)
			.Select(_ => Task.Run(async () =>
			{
				var response = await Client.PostAsync($"/events/{eventId}/book", null);
				responses.Add(response);
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		responses.Should().HaveCount(totalRequests);
		responses.Should().OnlyContain(x => x.StatusCode == HttpStatusCode.Accepted);
		responses.Select(x => x.Headers.Location).Distinct().Should().HaveCount(totalRequests);

		foreach (var response in responses)
		{
			var bookingByIdResponse = await Client.GetAsync(response.Headers.Location);
			bookingByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

			var responseData = await response.Content.ReadFromJsonAsync<BookingResponse>();
			responseData.Should().NotBeNull();
			responseData.EventId.Should().Be(eventId);
			responseData.Status.Should().Be(BookingStatus.Pending);
		}
	}

	/// <summary>
	/// Проверяет параллельное создание заявок на бронирование при овербукинге.
	/// </summary>
	[Fact]
	public async Task Book_MultipleOverbooking_Success()
	{
		//Arrange
		const int totalRequests = 25;
		var responses = new ConcurrentBag<HttpResponseMessage>();

		var eventId = await CreateEvent();

		//Act
		var tasks = Enumerable.Range(0, totalRequests)
			.Select(_ => Task.Run(async () =>
			{
				var response = await Client.PostAsync($"/events/{eventId}/book", null);
				responses.Add(response);
			})).ToArray();

		await Task.WhenAll(tasks);

		//Assert
		responses.Should().HaveCount(totalRequests);
		responses.Where(x => x.Headers.Location is not null).Select(x => x.Headers.Location)
			.Distinct().Should().HaveCount(TestData.TotalSeats);

		var acceptedResponses = responses.Where(x => x.StatusCode == HttpStatusCode.Accepted).ToArray();
		var conflictedResponses = responses.Where(x => x.StatusCode == HttpStatusCode.Conflict).ToArray();
		acceptedResponses.Should().HaveCount(TestData.TotalSeats);
		conflictedResponses.Should().HaveCount(totalRequests - TestData.TotalSeats);

		foreach (var response in acceptedResponses)
		{
			var bookingByIdResponse = await Client.GetAsync(response.Headers.Location);
			bookingByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

			var responseData = await response.Content.ReadFromJsonAsync<BookingResponse>();
			responseData.Should().NotBeNull();
			responseData.EventId.Should().Be(eventId);
			responseData.Status.Should().Be(BookingStatus.Pending);
		}
	}
}