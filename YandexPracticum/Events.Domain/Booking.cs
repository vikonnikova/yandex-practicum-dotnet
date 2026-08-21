namespace Events.Domain;

public class Booking
{
	private Booking(Guid id, Guid eventId, Guid userId, DateTime createdAt)
	{
		Id = id;
		EventId = eventId;
		UserId = userId;
		CreatedAt = createdAt;
		Status = BookingStatus.Pending;
	}

	private Booking()
	{
	}

	public Guid Id { get; init; }
	public Guid EventId { get; init; }
	public Event Event { get; private set; } = null!;
	public Guid UserId { get; init; }
	public User User { get; private set; } = null!;
	public DateTime CreatedAt { get; init; }
	public BookingStatus Status { get; private set; }
	public DateTime? ProcessedAt { get; private set; }

	public static Booking Create(Guid id, Guid eventId, Guid userId, DateTime createdAt)
	{
		return new Booking(id, eventId, userId, createdAt);
	}

	public void Confirm(DateTime processedAt)
	{
		Status = BookingStatus.Confirmed;
		ProcessedAt = processedAt;
	}

	public void Reject(DateTime processedAt)
	{
		Status = BookingStatus.Rejected;
		ProcessedAt = processedAt;
	}

	public void Cancel(DateTime processedAt)
	{
		Status = BookingStatus.Cancelled;
		ProcessedAt = processedAt; //TODO защита от повторной отмены
	}
}