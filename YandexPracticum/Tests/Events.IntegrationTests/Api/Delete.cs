using System.Net;

namespace Events.IntegrationTests.Api;

public class DeleteTests : BaseApiTest
{
	/// <summary>
	/// Проверяет успешное удаление события.
	/// </summary>
	[Fact]
	public async Task Delete_ValidData_200Returned()
	{
		//Arrange
		await CreateEvent();

		//Act
		var responseFromDelete = await Client.DeleteAsync("/events/1");

		//Assert
		Assert.Equal(HttpStatusCode.OK, responseFromDelete.StatusCode);
		var responseFromGet = await Client.GetAsync("/events/1");
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
		var responseFromDelete = await Client.DeleteAsync("/events/2");

		//Assert
		Assert.Equal(HttpStatusCode.NotFound, responseFromDelete.StatusCode);
	}
}