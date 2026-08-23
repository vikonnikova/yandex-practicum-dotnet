using Events.Api.Contracts.Users;

namespace Events.IntegrationTests;

internal static class TestData
{
	public static string Login => "vika_7486";
	public static string Password => "qwerty1234";
	public static UserRole Role => UserRole.User;
	
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
}