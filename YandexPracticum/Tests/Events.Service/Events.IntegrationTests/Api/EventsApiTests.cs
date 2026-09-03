using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Events.Api.Contracts.Events;
using Events.Domain;
using Events.IntegrationTests.Api.Base;
using FluentAssertions;
using Shared.Contracts;

namespace Events.IntegrationTests.Api;

public class EventsApiTests : BaseApiTest
{
    public EventsApiTests(ApiFixture fixture) : base(fixture)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(TestAuthHandler.AuthenticationScheme);
    }

    #region Get methods

    /// <summary>
    /// Проверяет получение всех событий.
    /// </summary>
    [Fact]
    public async Task GetAll_WhenValidData_ShouldReturn200()
    {
        //Arrange
        var events = CreateTestEvents();
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            foreach (var @event in events)
            {
                dbContext.Events.Add(@event);
            }
        });

        //Act
        var response = await Client.GetAsync("/events");

        //Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseData = (await response.Content.ReadFromJsonAsync<PaginatedResult<EventResponse>>())!;
        Assert.Equal(3, responseData.TotalItems);
    }

    /// <summary>
    /// Проверяет получение события по идентификатору.
    /// </summary>
    [Fact]
    public async Task GetById_WhenValidData_ShouldReturn200()
    {
        //Arrange
        var events = CreateTestEvents();
        var eventId = events[0].Id;
        await Fixture.ExecuteDbContextAsync(async dbContext =>

        {
            foreach (var @event in events)
            {
                dbContext.Events.Add(@event);
            }
        });

        //Act
        var response = await Client.GetAsync($"/events/{eventId}");

        //Assert
        var responseData = (await response.Content.ReadFromJsonAsync<EventResponse>())!;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(eventId, responseData.Id);
        Assert.Equal(TestData.Event1Title, responseData.Title);
        Assert.Equal(TestData.Event1Description, responseData.Description);
        Assert.Equal(TestData.Event1StartAt, responseData.StartAt);
        Assert.Equal(TestData.Event1EndAt, responseData.EndAt);
        Assert.Equal(TestData.Event1TotalSeats, responseData.TotalSeats);
    }

    /// <summary>
    /// Проверяет получение несуществующего события.
    /// </summary>
    [Fact]
    public async Task GetById_WhenNonExistentEvent_ShouldReturn404()
    {
        //Arrange
        var events = CreateTestEvents();
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            foreach (var @event in events)
            {
                dbContext.Events.Add(@event);
            }
        });

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
    public async Task Create_WhenValidData_ShouldReturn201()
    {
        //Act
        var response = await Client.PostAsJsonAsync("/events",
            new
            {
                Title = TestData.Event1Title,
                Description = TestData.Event1Description,
                StartAt = TestData.Event1StartAt,
                EndAt = TestData.Event1EndAt,
                TotalSeats = TestData.Event1TotalSeats
            });

        //Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var eventId = await response.Content.ReadFromJsonAsync<Guid>();
        Assert.Equal($"/Events/{eventId}", response.Headers.Location!.AbsolutePath);

        var createdEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!; //TODO dbContext
        Assert.Equal(eventId, createdEvent.Id);
        Assert.Equal(TestData.Event1Title, createdEvent.Title);
        Assert.Equal(TestData.Event1Description, createdEvent.Description);
        Assert.Equal(TestData.Event1StartAt, createdEvent.StartAt);
        Assert.Equal(TestData.Event1EndAt, createdEvent.EndAt);
    }

    /// <summary>
    /// Проверяет создание события с невалидными даными.
    /// </summary>
    [Fact]
    public async Task Create_WhenInvalidData_ShouldReturn400()
    {
        //Act
        var response = await Client.PostAsJsonAsync("/events",
            new { TestData.Event1Title, TestData.Event1Description, TestData.Event1StartAt, TestData.Event1EndAt });

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update method

    /// <summary>
    /// Проверяет обновление события.
    /// </summary>
    [Fact]
    public async Task Update_WhenValidData_ShouldReturn204()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(eventId, TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
        });

        //Act
        var response = await Client.PutAsJsonAsync($"/events/{eventId}",
            new
            {
                Title = TestData.UpdatedTitle,
                Description = TestData.UpdatedDescription,
                StartAt = TestData.UpdatedStartAt,
                EndAt = TestData.UpdatedEndAt,
                TotalSeats = TestData.UpdatedTotalSeats
            });

        //Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var updatedEvent = (await Client.GetFromJsonAsync<EventResponse>($"/events/{eventId}"))!; //TODO dbContext
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
    public async Task Update_WhenInvalidData_ShouldReturn400()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(eventId, TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
        });

        //Act
        var response = await Client.PutAsJsonAsync($"/events/{eventId}",
            new
            {
                Description = TestData.UpdatedDescription,
                StartAt = TestData.UpdatedStartAt,
                EndAt = TestData.UpdatedEndAt,
                TotalSeats = TestData.UpdatedTotalSeats
            });

        //Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Проверяет обновление несуществующего события.
    /// </summary>
    [Fact]
    public async Task Update_WhenNonExistentEvent_ShouldReturn404()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(Guid.NewGuid(), TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
        });

        //Act
        var response = await Client.PutAsJsonAsync($"/events/{Guid.NewGuid()}",
            new
            {
                Title = TestData.UpdatedTitle,
                Description = TestData.UpdatedDescription,
                StartAt = TestData.UpdatedStartAt,
                EndAt = TestData.UpdatedEndAt,
                TotalSeats = TestData.UpdatedTotalSeats
            });

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Delete method

    /// <summary>
    /// Проверяет успешное удаление события.
    /// </summary>
    [Fact]
    public async Task Delete_WhenValidData_ShouldReturn200()
    {
        //Arrange
        var eventId = Guid.NewGuid();
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(eventId, TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
        });

        //Act
        var responseFromDelete = await Client.DeleteAsync($"/events/{eventId}");

        //Assert
        Assert.Equal(HttpStatusCode.OK, responseFromDelete.StatusCode);
        var responseFromGet = await Client.GetAsync($"/events/{eventId}"); //TODO dbContext
        Assert.Equal(HttpStatusCode.NotFound, responseFromGet.StatusCode);
    }

    /// <summary>
    /// Проверяет удаление несуществующего события.
    /// </summary>
    [Fact]
    public async Task Delete_WhenNonExistentEvent_ShouldReturn404()
    {
        //Arrange
        await Fixture.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Events.Add(Event.Create(Guid.NewGuid(), TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats));
        });

        //Act
        var responseFromDelete = await Client.DeleteAsync($"/events/{Guid.NewGuid()}");

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, responseFromDelete.StatusCode);
    }

    #endregion

    private static Event[] CreateTestEvents()
    {
        return
        [
            Event.Create(Guid.NewGuid(), TestData.Event1Title, TestData.Event1Description,
                EventPeriod.Create(TestData.Event1StartAt, TestData.Event1EndAt), TestData.Event1TotalSeats),
            Event.Create(Guid.NewGuid(), TestData.Event2Title, TestData.Event2Description,
                EventPeriod.Create(TestData.Event2StartAt, TestData.Event2EndAt), TestData.Event2TotalSeats),
            Event.Create(Guid.NewGuid(), TestData.Event3Title, TestData.Event3Description,
                EventPeriod.Create(TestData.Event3StartAt, TestData.Event3EndAt), TestData.Event3TotalSeats)
        ];
    }
}