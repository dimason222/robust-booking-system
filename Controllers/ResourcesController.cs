using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Models;
using RobustBookingSystem.Services.Resources;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _resourceService;

        public ResourcesController(IResourceService resourceService)
        {
            _resourceService = resourceService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Resource>>> GetAll(CancellationToken ct)
        {
            var resources = await _resourceService.GetAllAsync(ct);
            return Ok(resources);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Resource>> GetById(int id, CancellationToken ct)
        {
            var resource = await _resourceService.GetByIdAsync(id, ct);

            if (resource is null)
                return NotFound(new { message = "Ресурс не знайдено." });

            return Ok(resource);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Resource>> Create([FromBody] Resource resource, CancellationToken ct)
        {
            var createdResource = await _resourceService.CreateAsync(resource, ct);

            return CreatedAtAction(nameof(GetById), new { id = createdResource.Id }, createdResource);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Resource>> Update(int id, [FromBody] UpdateResourceCommand command, CancellationToken ct)
        {
            var updated = await _resourceService.UpdateAsync(id, command, ct);
            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _resourceService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}