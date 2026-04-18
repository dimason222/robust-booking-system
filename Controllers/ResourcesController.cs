using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Data;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResourcesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Resource>>> GetAll(CancellationToken ct)
        {
            var resources = await _context.Resources.ToListAsync(ct);
            return Ok(resources);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Resource>> GetById(int id, CancellationToken ct)
        {
            var resource = await _context.Resources.FindAsync(new object[] { id }, ct);

            if (resource is null)
                return NotFound(new { message = "Ресурс не знайдено." });

            return Ok(resource);
        }

        [HttpPost]
        public async Task<ActionResult<Resource>> Create([FromBody] Resource resource, CancellationToken ct)
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = resource.Id }, resource);
        }
    }
}