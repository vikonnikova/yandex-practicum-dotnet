namespace Events.Domain;

public class Booking
{
	private Booking(Guid id, Guid eventId)
	{
		Id = id;
		EventId = eventId;
		Status = BookingStatus.Pending;
		CreatedAt = DateTime.UtcNow;
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? ProcessedAt { get; private set; }

	public static Booking Create(Guid id, Guid eventId)
	{
		return new Booking(id, eventId);
	}
}