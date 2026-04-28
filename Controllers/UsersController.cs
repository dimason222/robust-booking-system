using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobustBookingSystem.Dto.Auth;
using RobustBookingSystem.Models;
using RobustBookingSystem.Services.Users;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetAll(CancellationToken ct)
        {
            var users = await _userService.GetAllAsync(ct);
            return Ok(users);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<User>> GetById(int id, CancellationToken ct)
        {
            var user = await _userService.GetByIdAsync(id, ct);

            if (user is null)
                return NotFound(new { message = "Користувача не знайдено." });

            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create([FromBody] RegisterRequestDto dto, CancellationToken ct)
        {
            var result = await _userService.CreateAsync(dto, ct);

            if (!result.Success)
                return BadRequest(new { message = result.Error });

            return CreatedAtAction(nameof(GetById), new { id = result.User!.Id }, result.User);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<User>> Update(int id, [FromBody] RegisterRequestDto dto, CancellationToken ct)
        {
            var result = await _userService.UpdateAsync(id, dto, ct);

            if (!result.Success)
            {
                if (result.Error == "Користувача не знайдено.")
                    return NotFound(new { message = result.Error });

                return BadRequest(new { message = result.Error });
            }

            return Ok(result.User);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            await _userService.DeleteAsync(id, ct);
            return NoContent();
        }
    }
}