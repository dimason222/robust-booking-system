using RobustBookingSystem.Data;
using RobustBookingSystem.Models;
using RobustBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RobustBookingSystem.Repositories
{


    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _context.Bookings
                .Include(x => x.User)
                .Include(x => x.Resource)
                .FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<Booking>> GetAllAsync(CancellationToken ct = default) =>
            _context.Bookings
                .Include(x => x.User)
                .Include(x => x.Resource)
                .ToListAsync(ct);

        public Task<List<Booking>> GetByResourceAsync(int resourceId, CancellationToken ct = default) =>
            _context.Bookings
                .Include(x => x.User)
                .Include(x => x.Resource)
                .Where(x => x.ResourceId == resourceId)
                .ToListAsync(ct);

        public Task<List<Booking>> GetByUserAsync(int userId, CancellationToken ct = default) =>
            _context.Bookings
                .Include(x => x.User)
                .Include(x => x.Resource)
                .Where(x => x.UserId == userId)
                .ToListAsync(ct);

        public Task<bool> HasConflictAsync(int resourceId, DateTime startAtUtc, DateTime endAtUtc, CancellationToken ct = default) =>
            _context.Bookings.AnyAsync(x =>
                x.ResourceId == resourceId &&
                x.Status == BookingStatus.Active &&
                startAtUtc < x.EndAtUtc &&
                endAtUtc > x.StartAtUtc, ct);

        public async Task AddAsync(Booking booking, CancellationToken ct = default)
        {
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> HasConflictAsync(int resourceId, DateTime startAtUtc, DateTime endAtUtc, int excludeBookingId, CancellationToken ct = default)
        {
            return await _context.Bookings.AnyAsync(b =>
                b.Id != excludeBookingId &&
                b.ResourceId == resourceId &&
                b.Status == BookingStatus.Active &&
                startAtUtc < b.EndAtUtc &&
                endAtUtc > b.StartAtUtc,
                ct);
        }

        public async Task DeleteAsync(Booking booking, CancellationToken ct = default)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync(ct);
        }
    }
}
