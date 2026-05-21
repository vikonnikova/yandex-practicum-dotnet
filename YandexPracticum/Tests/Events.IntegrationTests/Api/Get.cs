using System.Net;
using System.Net.Http.Json;
using Events.Application.UseCases.Dto;

namespace Events.IntegrationTests.Api;

public class GetTests : BaseApiTest
{
	[Fact]
	public async Task GetAll_Success()
	{
		//Arrange
		await CreateEvents();

		//Act
		var response = await Client.GetAsync("/events");

		//Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);

		var responseData = (await response.Content.ReadFromJsonAsync<PaginatedResult<EventDto>>())!;
		Assert.Equal(3, responseData.TotalItems);
		// TODO дописать тест
	}

	[Fact]
	public async Task GetById_Success()
	{
		//Arrange
		await CreateEvent();

		//Act
		var response = await Client.GetAsync("/events/1");

		//Assert
		Assert.Equal(HttpStatusCode.OK, response.StatusCode);
		// TODO дописать тест
	}
}