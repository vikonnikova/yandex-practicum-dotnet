using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bookings.Api.Contracts.Bookings;
using Bookings.Domain;
using Bookings.IntegrationTests.Api.Base;
using FluentAssertions;
using BookingStatus = Bookings.Api.Contracts.Bookings.BookingStatus;

namespace Bookings.IntegrationTests.Api;

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
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Bookings.Add(Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId,
                DateTime.UtcNow));
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.GetAsync($"/bookings/{TestData.BookingId}");

        //Assert
        var responseData = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(TestData.BookingId, responseData.BookingId);
        Assert.Equal(TestData.EventId, responseData.EventId);
        //Assert.Equal(BookingStatus.Pending, responseData.Status);
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
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.GetAsync($"/bookings/{Guid.NewGuid()}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #region Book method

    /// <summary>
    /// Проверяет создание заявки на бронирование.
    /// </summary>
    [Fact]
    public async Task Book_WhenValidData_ShouldReturn202()
    {
        //Arrange

        //Act
        var response = await Client.PostAsync($"/events/{TestData.EventId}/book", null);

        //Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var booking = (await response.Content.ReadFromJsonAsync<BookingResponse>())!;
        Assert.Equal(new Uri($"/Bookings/{booking.BookingId}", UriKind.Relative), response.Headers.Location);
        Assert.Equal(TestData.EventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);

        var createdBooking =
            (await Client.GetFromJsonAsync<BookingResponse>($"/bookings/{booking.BookingId}"))!; //TODO dbContext
        Assert.Equal(booking.BookingId, createdBooking.BookingId);
        Assert.Equal(TestData.EventId, createdBooking.EventId);
        Assert.Equal(BookingStatus.Pending, createdBooking.Status);
    }

    /// <summary>
    /// Проверяет создание заявки на бронирование при овербукинге.
    /// </summary>
    [Fact]
    public async Task Book_WhenOverbooking_ShouldReturn409()
    {
        //Arrange
        var bookings = Enumerable.Range(0, 10)
            .Select(_ => Booking.Create(Guid.NewGuid(), TestData.EventId, TestData.UserId, DateTime.UtcNow)).ToArray();
        await Fixture.ExecuteDbContextAsync(async dbContext => { dbContext.Bookings.AddRange(bookings); });

        //Act
        var response = await Client.PostAsync($"/events/{TestData.EventId}/book", null);

        //Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    /// <summary>
    /// Проверяет создание заявки на бронирование на несуществующего пользователя.
    /// </summary>
    [Fact]
    public async Task Book_WhenNonExistentUser_ShouldReturn404()
    {
        //Act
        var response = await Client.PostAsync($"/events/{Guid.NewGuid()}/book", null);

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Проверяет создание заявки на бронирование на несуществующее событие.
    /// </summary>
    [Fact]
    public async Task Book_WhenNonExistentEvent_ShouldReturn404()
    {
        //Act
        var response = await Client.PostAsync($"/events/{Guid.NewGuid()}/book", null);

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Проверяет параллельное создание заявок на бронирование.
    /// </summary>
    [Fact]
    public async Task Book_WhenMultipleValidRequests_ShouldWorkCorrectly()
    {
        //Arrange
        var totalRequests = 10;
        var responses = new ConcurrentBag<HttpResponseMessage>();
        var eventId = Guid.NewGuid();

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
            var bookingByIdResponse = await Client.GetAsync(response.Headers.Location); //TODO dbContext
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
    public async Task Book_WhenMultipleRequestsCauseOverbooking_ShouldWorkCorrectly()
    {
        //Arrange
        const int totalRequests = 25;
        var responses = new ConcurrentBag<HttpResponseMessage>();
        var eventId = Guid.NewGuid();

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
            .Distinct().Should().HaveCount(10);

        var acceptedResponses = responses.Where(x => x.StatusCode == HttpStatusCode.Accepted).ToArray();
        var conflictedResponses = responses.Where(x => x.StatusCode == HttpStatusCode.Conflict).ToArray();
        acceptedResponses.Should().HaveCount(10);
        conflictedResponses.Should().HaveCount(totalRequests - 10);

        foreach (var response in acceptedResponses)
        {
            var bookingByIdResponse = await Client.GetAsync(response.Headers.Location); //TODO dbContext
            bookingByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var responseData = await response.Content.ReadFromJsonAsync<BookingResponse>();
            responseData.Should().NotBeNull();
            responseData.EventId.Should().Be(eventId);
            responseData.Status.Should().Be(BookingStatus.Pending);
        }
    }

    #endregion

    #region Cancel method

    /// <summary>
    /// Проверяет получение несуществующего бронирования.
    /// </summary>
    [Fact]
    public async Task Cancel_WhenValidData_ShouldReturn200()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Bookings.Add(Booking.Create(TestData.BookingId, TestData.EventId, TestData.UserId,
                DateTime.UtcNow));
            await dbContext.SaveChangesAsync(CancellationToken.None);
        });

        //Act
        var response = await Client.DeleteAsync($"/bookings/{TestData.BookingId}");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    #endregion
}