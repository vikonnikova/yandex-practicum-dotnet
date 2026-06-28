using System.Net;

namespace Events.IntegrationTests.EventsController;

public class DeleteTests(ApiWebApplicationFactory factory) : BaseApiTest(factory)
{
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
}