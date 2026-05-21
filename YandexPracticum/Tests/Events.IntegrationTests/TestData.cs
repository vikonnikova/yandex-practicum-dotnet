namespace Events.IntegrationTests;

public static class TestData
{
	public static string Title => "Наименование";
	public static string Description => "Описание";
	public static DateTime StartAt => new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
	public static DateTime EndAt => new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
	
	public static string UpdatedTitle => "Новое наименование";
	public static string UpdatedDescription => "Новое описание";
	public static DateTime UpdatedStartAt => new(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc);
	public static DateTime UpdatedEndAt => new(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc);

	public static object CreateTestEvent()
	{
		return new
		{
			Id = 1,
			Title = "Наименование",
			Description = "Описание",
			StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc)
		};
	}

	public static object CreateTestEventToUpdate()
	{
		return new
		{
			Title = "Новое наименование",
			Description = "Новое описание",
			StartAt = new DateTime(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc),
			EndAt = new DateTime(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc)
		};
	}

	public static object[] CreateTestEvents()
	{
		return new[]
		{
			new
			{
				Id = 1,
				Title = "Наименование1",
				Description = "Описание1",
				StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc)
			},

			new
			{
				Id = 2,
				Title = "Наименование2",
				Description = "Описание2",
				StartAt = new DateTime(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc)
			},
			new
			{
				Id = 3,
				Title = "Наименование3",
				Description = "Описание3",
				StartAt = new DateTime(2026, 03, 03, 15, 32, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 03, 03, 17, 52, 00, DateTimeKind.Utc)
			}
		};
	}
}