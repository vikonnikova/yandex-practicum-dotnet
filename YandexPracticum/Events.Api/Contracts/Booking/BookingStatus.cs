namespace Events.Api.Contracts;

/// <summary>
/// Представляет статусы бронирования.
/// </summary>
public enum BookingStatus
{
	/// <summary>
	/// В ожидании.
	/// </summary>
	Pending,

	/// <summary>
	/// Бронирование подтверждено.
	/// </summary>
	Confirmed,

	/// <summary>
	/// Бронирование отклонено.
	/// </summary>
	Rejected
}