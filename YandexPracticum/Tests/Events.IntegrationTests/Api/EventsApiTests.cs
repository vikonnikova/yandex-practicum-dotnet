using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;
using Events.IntegrationTests.Api.Base;
using FluentAssertions;

namespace Events.IntegrationTests.Api;

public class EventsApiTests(ApiFixture fixture) : BaseApiTest(fixture)
{
	#region Get methods

	/// <summary>
	/// Проверяет получение всех событий.
	/// </summary>
	[Fact]
	public async Task GetAll_Success()
	{
		//Arrange
		await CreateEvents();

		//Act
		var response = await Client.GetAsync("/events");

		//Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		var responseData = (await response.Content.ReadFromJsonAsync<PaginatedResult<EventResponse>>())!;
		Assert.Equal(3, responseData.Meta.TotalItems);
	}

	/// <summary>
	/// Проверяет получение события по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_ValidData_200Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var response = await Client.GetAsync($"/events/{eventId}");

		//Assert
		var responseData = (await response.Content.ReadFromJsonAsync<EventResponse>())!;
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		Assert.Equal(eventId, responseData.Id);
		Assert.Equal(TestData.Title, responseData.Title);
		Assert.Equal(TestData.Description, responseData.Description);
		Assert.Equal(TestData.StartAt, responseData.StartAt);
		Assert.Equal(TestData.EndAt, responseData.EndAt);
	}

	/// <summary>
	/// Проверяет получение несуществующего события.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentEvent_404Returned()
	{
		//Arrange
		await CreateEvents();

		//Act
		var response = await Client.GetAsync($"/events/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	#endregion

	#region Create method

	/// <summary>
	/// Проверяет создание события.
	/// </summary>
	[Fact]
	public async Task Create_ValidData_201Returned()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/events", CreateTestEvent());

		//Assert
		var responseData = (await response.Content.ReadFromJsonAsync<EventResponse>())!;
		var eventId = responseData.Id;
		Assert.Equal(HttpStatusCode.Created, response.StatusCode);
		Assert.Equal($"/Events/{eventId}", response.Headers.Location!.AbsolutePath);

		Assert.Equal(TestData.Title, responseData.Title);
		Assert.Equal(TestData.Description, responseData.Description);
		Assert.Equal(TestData.StartAt, responseData.StartAt);
		Assert.Equal(TestData.EndAt, responseData.EndAt);

		var createdEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!;
		Assert.Equal(eventId, createdEvent.Id);
		Assert.Equal(TestData.Title, createdEvent.Title);
		Assert.Equal(TestData.Description, createdEvent.Description);
		Assert.Equal(TestData.StartAt, createdEvent.StartAt);
		Assert.Equal(TestData.EndAt, createdEvent.EndAt);
	}

	/// <summary>
	/// Проверяет создание события с невалидными даными.
	/// </summary>
	[Fact]
	public async Task Create_InvalidData_400Returned()
	{
		//Act
		var response = await Client.PostAsJsonAsync("/events", CreateInvalidTestEvent());

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	#endregion

	#region Update method

	/// <summary>
	/// Проверяет обновление события.
	/// </summary>
	[Fact]
	public async Task Update_ValidData_204Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{eventId}", CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

		var updatedEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!;
		Assert.Equal(eventId, updatedEvent.Id);
		Assert.Equal(TestData.UpdatedTitle, updatedEvent.Title);
		Assert.Equal(TestData.UpdatedDescription, updatedEvent.Description);
		Assert.Equal(TestData.UpdatedStartAt, updatedEvent.StartAt);
		Assert.Equal(TestData.UpdatedEndAt, updatedEvent.EndAt);
	}

	/// <summary>
	/// Проверяет обновление события с невалидными данными.
	/// </summary>
	[Fact]
	public async Task Update_InvalidData_400Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{eventId}", CreateInvalidTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
	}

	/// <summary>
	/// Проверяет обновление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Update_NonExistentEvent_404Returned()
	{
		//Arrange
		await CreateEvent();

		//Act
		var response = await Client.PutAsJsonAsync($"/events/{Guid.NewGuid()}", CreateTestEventToUpdate());

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}

	#endregion

	#region Delete method

	/// <summary>
	/// Проверяет успешное удаление события.
	/// </summary>
	[Fact]
	public async Task Delete_ValidData_200Returned()
	{
		//Arrange
		var eventId = await CreateEvent();

		//Act
		var responseFromDelete = await Client.DeleteAsync($"/events/{eventId}");

		//Assert
		Assert.Equal(HttpStatusCode.OK, responseFromDelete.StatusCode);
		var responseFromGet = await Client.GetAsync($"/events/{eventId}");
		Assert.Equal(HttpStatusCode.NotFound, responseFromGet.StatusCode);
	}

	/// <summary>
	/// Проверяет удаление несуществующего события.
	/// </summary>
	[Fact]
	public async Task Delete_NonExistentEvent_404Returned()
	{
		//Arrange
		await CreateEvent();

		//Act
		var responseFromDelete = await Client.DeleteAsync($"/events/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, responseFromDelete.StatusCode);
	}

	#endregion

	#region Book method

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
		var bookedEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!;
		Assert.Equal(TestData.TotalSeats, bookedEvent.TotalSeats);
		Assert.Equal(TestData.TotalSeats - 1, bookedEvent.AvailableSeats);
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

	#endregion
}