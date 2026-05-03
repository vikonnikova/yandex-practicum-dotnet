namespace Events.Domain;

public class Event
{
	private Event(int id, string title, string? description, DateTime startAt, DateTime endAt)
	{
		Id = id;
		Title = title;
		Description = description;
		StartAt = startAt;
		EndAt = endAt;
	}

	public int Id { get; private set; }
	public string Title { get; private set; }
	public string? Description { get; private set; }
	public DateTime StartAt { get; private set; }
	public DateTime EndAt { get; private set; }

	public static Event Create(int id, string title, string? description, DateTime startAt, DateTime endAt)
	{
		return new Event(id, title, description, startAt, endAt);
	}

	public void Update(string title, string? description, DateTime startAt, DateTime endAt)
	{
		Title = title;
		Description = description;
		StartAt = startAt;
		EndAt = endAt;
	}
}