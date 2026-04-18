using RobustBookingSystem.Models;

namespace RobustBookingSystem.Repositories.Interfaces
{
    public interface IResourceRepository
    {
        Task<Resource?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Resource>> GetAllAsync(CancellationToken ct = default);
    }
}
