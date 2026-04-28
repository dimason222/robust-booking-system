using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Data;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Exceptions;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Services.Resources
{
    public class ResourceService : IResourceService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(AppDbContext context, ILogger<ResourceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Resource>> GetAllAsync(CancellationToken ct)
        {
            _logger.LogInformation("Request to get all resources");

            var resources = await _context.Resources.ToListAsync(ct);

            _logger.LogInformation("Retrieved {Count} resources", resources.Count);

            return resources;
        }

        public async Task<Resource?> GetByIdAsync(int id, CancellationToken ct)
        {
            _logger.LogInformation("Request to get resource {ResourceId}", id);

            var resource = await _context.Resources.FindAsync(new object[] { id }, ct);

            if (resource is null)
            {
                _logger.LogWarning("Resource {ResourceId} was not found", id);
                return null;
            }

            return resource;
        }

        public async Task<Resource> CreateAsync(Resource resource, CancellationToken ct)
        {
            _logger.LogInformation(
                "Attempt to create resource with name {ResourceName}",
                resource.Name);

            try
            {
                _context.Resources.Add(resource);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Resource {ResourceId} created successfully",
                    resource.Id);

                return resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating resource with name {ResourceName}",
                    resource.Name);

                throw;
            }
        }
        public async Task<Resource> UpdateAsync(int id, UpdateResourceCommand command, CancellationToken ct)
        {
            _logger.LogInformation(
                "Attempt to update resource {ResourceId}",
                id);

            var resource = await _context.Resources.FindAsync(new object[] { id }, ct);

            if (resource is null)
            {
                _logger.LogWarning(
                    "Resource {ResourceId} was not found for update",
                    id);

                throw new NotFoundException("Ресурс не знайдено.");
            }

            if (string.IsNullOrWhiteSpace(command.Name))
            {
                _logger.LogWarning(
                    "Resource {ResourceId} update failed: empty name",
                    id);

                throw new ValidationException("Назва ресурсу не може бути порожньою.");
            }

            if (command.Capacity <= 0)
            {
                _logger.LogWarning(
                    "Resource {ResourceId} update failed: invalid capacity {Capacity}",
                    id,
                    command.Capacity);

                throw new ValidationException("Місткість ресурсу повинна бути більшою за нуль.");
            }

            resource.Name = command.Name;
            resource.Description = command.Description;
            resource.Location = command.Location;
            resource.Capacity = command.Capacity;

            try
            {
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Resource {ResourceId} updated successfully",
                    resource.Id);

                return resource;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while updating resource {ResourceId}",
                    id);

                throw;
            }
        }
        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            _logger.LogInformation("Attempt to delete resource {ResourceId}", id);

            var resource = await _context.Resources
                .Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (resource is null)
            {
                _logger.LogWarning("Resource {ResourceId} was not found for delete", id);
                throw new NotFoundException("Ресурс не знайдено.");
            }

            var hasBookings = resource.Bookings.Any();

            if (hasBookings)
            {
                _logger.LogWarning(
                    "Resource {ResourceId} cannot be deleted because it has bookings",
                    id);

                throw new ValidationException("Ресурс не можна видалити, оскільки він має пов'язані бронювання.");
            }

            try
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation("Resource {ResourceId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting resource {ResourceId}", id);
                throw;
            }
        }
    }
}