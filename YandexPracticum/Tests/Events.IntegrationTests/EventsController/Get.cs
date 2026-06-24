using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;

namespace Events.IntegrationTests.EventsController;

public class GetTests(ApiWebApplicationFactory factory) : BaseApiTest(factory)
{
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
}