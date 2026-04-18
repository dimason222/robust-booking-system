using RobustBookingSystem.Data;
using RobustBookingSystem.Models;
using RobustBookingSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RobustBookingSystem.Repositories
{


    public class ResourceRepository : IResourceRepository
    {
        private readonly AppDbContext _context;

        public ResourceRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Resource?> GetByIdAsync(int id, CancellationToken ct = default) =>
            _context.Resources.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<List<Resource>> GetAllAsync(CancellationToken ct = default) =>
            _context.Resources.ToListAsync(ct);
    }
}
