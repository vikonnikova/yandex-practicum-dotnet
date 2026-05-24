using System.Net;
using System.Net.Http.Json;
using Events.Api.Contracts;

namespace Events.IntegrationTests.Api;

public class GetTests : BaseApiTest
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
		await CreateEvent();

		//Act
		var response = await Client.GetAsync("/events/1");

		//Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
	}
	
	/// <summary>
	/// Проверяет получение несуществующего события.
	/// </summary>
	[Fact]
	public async Task GetById_NonExistentEvent_404Returned()
	{
		//Arrange
		await CreateEvent();

		//Act
		var response = await Client.GetAsync("/events/2");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
	}
}