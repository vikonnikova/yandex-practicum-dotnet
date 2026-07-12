namespace Events.IntegrationTests.Api.Base;

internal static class TestData
{
	public static string Title => "Ярмарка мёда";
	public static string Description => "Большой ассортимент мёда на главной площади города.";
	public static DateTime StartAt => new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
	public static DateTime EndAt => new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
	public static int TotalSeats => 10;
	
	public static string UpdatedTitle => "Яблочный спас";
	public static string UpdatedDescription => "Народное название праздника Преображение Господне";
	public static DateTime UpdatedStartAt => new(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc);
	public static DateTime UpdatedEndAt => new(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc);
	public static int UpdatedTotalSeats => 15;

	public static object CreateTestEvent()
	{
		return new { Title, Description, StartAt, EndAt, TotalSeats };
	}
	
	public static object CreateInvalidTestEvent()
	{
		return new { Title, Description, StartAt, EndAt };
	}

	public static object CreateTestEventToUpdate()
	{
		return new
		{
			Title = UpdatedTitle,
			Description = UpdatedDescription,
			StartAt = UpdatedStartAt,
			EndAt = UpdatedEndAt,
			TotalSeats = UpdatedTotalSeats
		};
	}
	
	public static object CreateInvalidTestEventToUpdate()
	{
		return new
		{
			Description = UpdatedDescription,
			StartAt = UpdatedStartAt,
			EndAt = UpdatedEndAt,
			TotalSeats = UpdatedTotalSeats
		};
	}

	public static object[] CreateTestEvents()
	{
		return
		[
			new
			{
				Title = "Оперетта",
				Description = "Музыкально-театральный жанр, сочетающий вокальное и драматическое искусство, хореографию и разговорные диалоги",
				StartAt = new DateTime(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc),
				TotalSeats = 30
			},

			new
			{
				Title = "Балет",
				Description = "Театральный спектакль, в котором сюжет, характеры и эмоции героев передаются без слов — с помощью танца, пластики и музыки",
				StartAt = new DateTime(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc),
				TotalSeats = 40
			},
			new
			{
				Title = "Кукольный театр",
				Description = "Форма театра или представления, в которой используются куклы",
				StartAt = new DateTime(2026, 03, 03, 15, 32, 00, DateTimeKind.Utc),
				EndAt = new DateTime(2026, 03, 03, 17, 52, 00, DateTimeKind.Utc),
				TotalSeats = 50
			}
		];
	}
}