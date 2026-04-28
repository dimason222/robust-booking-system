using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Services.Resources
{
    public interface IResourceService
    {
        Task<List<Resource>> GetAllAsync(CancellationToken ct);
        Task<Resource?> GetByIdAsync(int id, CancellationToken ct);
        Task<Resource> CreateAsync(Resource resource, CancellationToken ct);
        Task<Resource> UpdateAsync(int id, UpdateResourceCommand command, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}