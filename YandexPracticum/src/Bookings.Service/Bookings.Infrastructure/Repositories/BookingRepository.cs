using Bookings.Application.Interfaces;
using Bookings.Domain;
using Bookings.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts;

namespace Bookings.Infrastructure.Repositories;

internal class BookingRepository(BookingsDbContext context) : IBookingRepository
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

    public async Task<int> CountPendingByUser(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Bookings.CountAsync(x => x.Status == BookingStatus.Pending && x.UserId == userId,
            cancellationToken);
    }

    public async Task<PaginatedResult<Booking>> GetByUser(Guid userId, int page, int pageSize,
        CancellationToken cancellationToken)
    {
        var query = context.Bookings
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt);

        var totalItems = await query.CountAsync(cancellationToken);
        var result = await query.Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(cancellationToken);

        return new PaginatedResult<Booking>(result, totalItems);
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