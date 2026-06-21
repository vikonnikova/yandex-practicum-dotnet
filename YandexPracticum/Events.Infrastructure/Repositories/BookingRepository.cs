using Events.Application.Interfaces;
using Events.Domain;
using Events.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
	public async Task<Booking?> Find(Guid bookingId, CancellationToken cancellationToken)
	{
		return await context.Bookings.FindAsync([bookingId], cancellationToken: cancellationToken);
	}

	public async Task<IReadOnlyCollection<Booking>> GetPending(CancellationToken cancellationToken)
	{
		return await context.Bookings.Where(b => b.Status == BookingStatus.Pending).ToArrayAsync(cancellationToken);
	}

	public async Task Add(Booking booking, CancellationToken cancellationToken)
	{
		context.Bookings.Add(booking);
		await context.SaveChangesAsync(cancellationToken);
	}
}