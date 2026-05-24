using System.Net.Http.Json;

namespace Events.IntegrationTests.Api;

public class BaseApiTest : /*IClassFixture<ApiWebApplicationFactory>,*/ IDisposable
{
	protected readonly ApiWebApplicationFactory Factory;
	protected readonly HttpClient Client;

	protected BaseApiTest()
	{
		Factory = new ApiWebApplicationFactory();
		Client = Factory.CreateClient();
	}

	public void Dispose()
	{
		Client.Dispose();
		Factory.Dispose();
	}

	protected async Task CreateEvent()
	{
		await Client.PostAsJsonAsync("/events", TestData.CreateTestEvent());
	}

	protected async Task CreateEvents()
	{
		foreach (var @event in TestData.CreateTestEvents())
		{
			await Client.PostAsJsonAsync("/events", @event);
		}
	}
}