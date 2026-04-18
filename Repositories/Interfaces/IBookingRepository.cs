using RobustBookingSystem.Models;

namespace RobustBookingSystem.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Booking>> GetByResourceAsync(int resourceId, CancellationToken ct = default);
        Task<List<Booking>> GetByUserAsync(int userId, CancellationToken ct = default);
        Task<bool> HasConflictAsync(int resourceId, DateTime startAtUtc, DateTime endAtUtc, CancellationToken ct = default);
        Task AddAsync(Booking booking, CancellationToken ct = default);
        Task UpdateAsync(Booking booking, CancellationToken ct = default);

        Task<List<Booking>> GetAllAsync(CancellationToken ct = default);
    }
}
