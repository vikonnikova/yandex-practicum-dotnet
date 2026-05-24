namespace Events.Domain;

public class EventPeriod
{
	public DateTime StartAt { get; private set; }
	public DateTime EndAt { get; private set; }

	private EventPeriod(DateTime startAt, DateTime endAt)
	{
		StartAt = startAt;
		EndAt = endAt;
	}

	public static EventPeriod Create(DateTime startAt, DateTime endAt)
	{
		if (startAt == default)
		{
			throw new ArgumentNullException(nameof(startAt));
		}
		
		if (endAt == default)
		{
			throw new ArgumentNullException(nameof(endAt));
		}
		
		if (endAt <= startAt)
		{
			throw new ArgumentException("Начало события должно быть раньше его завершения.");
		}

		return new EventPeriod(startAt, endAt);
	}
}