using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

internal class BookingRepository(AppDbContext context) : IBookingRepository
{
	public async Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken)
	{
		return await context.Bookings.FindAsync([bookingId], cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyCollection<Guid>> GetPending(CancellationToken cancellationToken)
	{
		return await context.Bookings.Where(b => b.Status == BookingStatus.Pending).Select(x => x.Id)
			.ToArrayAsync(cancellationToken);
	}

	public async Task<int> CountBy(Guid eventId, Guid userId, CancellationToken cancellationToken)
	{
		return await context.Bookings.CountAsync(x => x.EventId == eventId && x.UserId == userId, cancellationToken);
	}

	public void Add(Booking booking)
	{
		context.Bookings.Add(booking);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}