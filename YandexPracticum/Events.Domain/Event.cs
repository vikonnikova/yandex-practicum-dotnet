namespace Events.Domain;

public class Event
{
    private Event(Guid id, string title, string? description, EventPeriod period, int totalSeats)
    {
        Id = id;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        Period = period ?? throw new ArgumentNullException(nameof(period));
        if (totalSeats < 1)
        {
            throw new ArgumentException("Общее количество мест должно быть больше нуля.");
        }

        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    private Event()
    {
    }

    public Guid Id { get; init; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public EventPeriod Period { get; private set; } = null!;
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }

    public IReadOnlyCollection<Booking> Bookings { get; private set; } = null!;

    public static Event Create(Guid id, string title, string? description, EventPeriod period, int totalSeats)
    {
        return new Event(id, title, description, period, totalSeats);
    }

    public void Update(string title, string? description, EventPeriod period)
    {
        Title = title;
        Description = description;
        Period = period;
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats += count;
    }
}