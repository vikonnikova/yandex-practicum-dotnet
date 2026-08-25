using Events.Domain;

namespace Events.IntegrationTests;

internal static class TestData
{
	public static readonly Guid UserId = Guid.NewGuid();
	public const string Login = "vika_7486";
	public const string Password = "qwerty1234";

	public static Guid EventId = Guid.NewGuid();
	public const string Event1Title = "Ярмарка мёда";
	public const string Event1Description = "Большой ассортимент мёда на главной площади города.";
	public static DateTime Event1StartAt = new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
	public static DateTime Event1EndAt = new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
	public const int Event1TotalSeats = 10;

	public const string Event2Title = "Оперетта";
	public const string Event2Description = "Музыкально-театральный жанр.";
	public static DateTime Event2StartAt = new(2026, 01, 01, 10, 30, 00, DateTimeKind.Utc);
	public static DateTime Event2EndAt = new(2026, 01, 01, 12, 45, 00, DateTimeKind.Utc);
	public const int Event2TotalSeats = 30;

	public const string Event3Title = "Балет";
	public const string Event3Description = "Театральный спектакль.";
	public static DateTime Event3StartAt = new(2026, 02, 02, 03, 20, 00, DateTimeKind.Utc);
	public static DateTime Event3EndAt = new(2026, 02, 02, 22, 30, 00, DateTimeKind.Utc);
	public const int Event3TotalSeats = 50;

	public const string UpdatedTitle = "Яблочный спас";
	public const string UpdatedDescription = "Народное название праздника Преображение Господне";
	public static DateTime UpdatedStartAt = new(2026, 02, 03, 18, 55, 00, DateTimeKind.Utc);
	public static DateTime UpdatedEndAt = new(2026, 02, 05, 12, 00, 00, DateTimeKind.Utc);
	public const int UpdatedTotalSeats = 15;
	
	public static Guid BookingId = Guid.NewGuid();

	public static User TestUser => User.Create(UserId, Login, Password, UserRole.User);

	public static Event TestEvent => Event.Create(EventId, Event1Title, Event1Description,
		EventPeriod.Create(Event1StartAt, Event1EndAt), Event1TotalSeats);
}