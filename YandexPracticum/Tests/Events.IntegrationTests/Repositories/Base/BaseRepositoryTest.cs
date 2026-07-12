using Events.Domain;
using Events.IntegrationTests.Infrastructure;

namespace Events.IntegrationTests.Repositories.Base;

public abstract class BaseRepositoryTest(DbFixture dbFixture) : IAsyncLifetime
{
	protected readonly DbFixture DbFixture = dbFixture;
	protected readonly DateTime Now = DateTime.UtcNow;
	protected readonly Guid EventId = Guid.NewGuid();
	protected readonly Guid BookingId = Guid.NewGuid();

	protected Event CreateEvent()
	{
		return Event.Create(EventId, TestData.Title, TestData.Description,
			EventPeriod.Create(TestData.StartAt, TestData.EndAt), TestData.TotalSeats);
	}

	protected async Task SeedData()
	{
		await using var context = DbFixture.CreateContext();


		context.Events.AddRange(
			Event.Create(Guid.NewGuid(), "День рождения", "Дед Мороз и снегурочка",
				EventPeriod.Create(Now, Now.AddDays(7)), 17),
			Event.Create(Guid.NewGuid(), "Пасха", "Красим яйца, печем куличи",
				EventPeriod.Create(Now.AddHours(-12), Now.AddHours(-10)), 20),
			Event.Create(Guid.NewGuid(), "Рождество", "описание рождества, подарки, игрушки",
				EventPeriod.Create(Now.AddMonths(-5), Now.AddMonths(-5).AddDays(2)), 100),
			Event.Create(Guid.NewGuid(), "23 февраля", "День защитника отечества",
				EventPeriod.Create(Now.AddDays(-7), Now.AddDays(-6)), 5),
			Event.Create(Guid.NewGuid(), "День победы", "Парад и салют",
				EventPeriod.Create(Now, Now.AddHours(14)), 7)
		);

		await context.SaveChangesAsync();
	}

	public Task InitializeAsync()
	{
		return Task.CompletedTask;
	}

	public async Task DisposeAsync()
	{
		await DbFixture.ClearTablesAsync();
	}
}