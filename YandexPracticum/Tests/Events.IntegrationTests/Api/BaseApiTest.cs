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
		var @event = new
		{
			Id = 1,
			Title = "Наименование",
			Description = "Описание",
			StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc)
		};
		await Client.PostAsJsonAsync("/events", @event);
	}

	protected async Task CreateEvents()
	{
		var @event = new
		{
			Id = 1,
			Title = "Наименование1",
			Description = "Описание1",
			StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc)
		};
		await Client.PostAsJsonAsync("/events", @event);

		@event = new
		{
			Id = 2,
			Title = "Наименование2",
			Description = "Описание2",
			StartAt = new DateTime(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc)
		};
		await Client.PostAsJsonAsync("/events", @event);

		@event = new
		{
			Id = 3,
			Title = "Наименование3",
			Description = "Описание3",
			StartAt = new DateTime(2026, 03, 03, 15, 32, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 03, 03, 17, 52, 00, DateTimeKind.Utc)
		};
		await Client.PostAsJsonAsync("/events", @event);
	}
}