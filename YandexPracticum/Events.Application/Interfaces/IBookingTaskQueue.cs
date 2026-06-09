namespace Events.Application.Interfaces;

public interface IBookingTaskQueue
{
	void Enqueue(BookingTask task);
	bool TryDequeue(out BookingTask task);
}