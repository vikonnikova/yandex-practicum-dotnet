namespace Events.Domain;

public class Booking
{
	private Booking(Guid id, Guid eventId, DateTime createdAt)
	{
		Id = id;
		EventId = eventId;
		CreatedAt = createdAt;
		Status = BookingStatus.Pending;
	}

	private Booking()
	{
	}

	public Guid Id { get; private set; }
	public Guid EventId { get; private set; }
	public Event Event { get; private set; } = null!;
	public BookingStatus Status { get; private set; }
	public DateTime CreatedAt { get; private set; }
	public DateTime? ProcessedAt { get; private set; }

	public static Booking Create(Guid id, Guid eventId, DateTime createdAt)
	{
		return new Booking(id, eventId, createdAt);
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
}