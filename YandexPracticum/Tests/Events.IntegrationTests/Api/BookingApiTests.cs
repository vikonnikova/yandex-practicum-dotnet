using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Events.Api.Contracts.Bookings;
using Events.Domain;
using Events.IntegrationTests.Api.Base;
using BookingStatus = Events.Api.Contracts.Bookings.BookingStatus;

namespace Events.IntegrationTests.Api;

public class BookingApiTests : BaseApiTest
{
	public BookingApiTests(ApiFixture fixture) : base(fixture)
	{
		Client.DefaultRequestHeaders.Authorization =
			new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
	}

	/// <summary>
	/// Проверяет получение брони по идентификатору.
	/// </summary>
	[Fact]
	public async Task GetById_WhenValidData_ShouldReturn200()
	{
		//Arrange
		var eventId = Guid.NewGuid();
		var bookingId = Guid.NewGuid();
		await Fixture.ExecuteDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(User.Create(TestData.UserId, TestData.Login, TestData.Password, UserRole.User));
			dbContext.Events.Add(Event.Create(eventId, TestData.Event1Title, TestData.Event1Description,
				EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
			dbContext.Bookings.Add(Booking.Create(bookingId, eventId, TestData.UserId, DateTime.UtcNow));
			await dbContext.SaveChangesAsync(CancellationToken.None);
		});

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
	public async Task GetById_WhenNonExistentBooking_ShouldReturn404()
	{
		//Arrange
		await Fixture.ExecuteDbContextAsync(async dbContext =>
		{
			dbContext.Users.Add(User.Create(Guid.NewGuid(), TestData.Login, TestData.Password, UserRole.User));
			dbContext.Events.Add(Event.Create(Guid.NewGuid(), TestData.Event1Title, TestData.Event1Description,
				EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
			await dbContext.SaveChangesAsync(CancellationToken.None);
		});

		//Act
		var response = await Client.GetAsync($"/bookings/{Guid.NewGuid()}");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}