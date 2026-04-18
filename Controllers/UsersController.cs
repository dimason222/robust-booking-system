using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Data;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll(CancellationToken ct)
        {
            var users = await _context.Users.ToListAsync(ct);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id, CancellationToken ct)
        {
            var user = await _context.Users.FindAsync(new object[] { id }, ct);

            if (user is null)
                return NotFound(new { message = "Користувача не знайдено." });

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] User user, CancellationToken ct)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
    }
}