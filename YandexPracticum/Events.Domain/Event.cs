namespace Events.Domain;

public class Event
{
	private Event(int id, string title, string? description, EventPeriod period)
	{
		Id = id;
		Title = title ?? throw new ArgumentNullException(nameof(title));
		Description = description;
		Period = period ?? throw new ArgumentNullException(nameof(period));
	}

	public int Id { get; private set; }
	public string Title { get; private set; }
	public string? Description { get; private set; }
	public EventPeriod Period { get; private set; }

	public static Event Create(int id, string title, string? description, EventPeriod period)
	{
		return new Event(id, title, description, period);
	}

	public void Update(string title, string? description, EventPeriod period)
	{
		Title = title;
		Description = description;
		Period = period;
	}
}