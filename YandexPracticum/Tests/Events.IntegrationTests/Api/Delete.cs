using System.Net;

namespace Events.IntegrationTests.Api;

public class DeleteTests : BaseApiTest
{
	[Fact]
	public async Task Delete_Success()
	{
		//Arrange
		await CreateEvent();

		//Act
		var responseFromDelete = await Client.DeleteAsync($"/events/1");

		//Assert
		Assert.Equal(HttpStatusCode.OK, responseFromDelete.StatusCode);
		var responseFromGet = await Client.GetAsync($"/events/1");
		Assert.Equal(HttpStatusCode.NotFound, responseFromGet.StatusCode);
	}
}