using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Dto.Responses;
using RobustBookingSystem.Services.Interfaces;
using System.Security.Claims;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BookingDto>>> GetAll(CancellationToken ct)
        {
            var result = await _bookingService.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookingDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _bookingService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("my")]
        public async Task<ActionResult<List<BookingDto>>> GetMy(CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _bookingService.GetMyBookingsAsync(userId, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingCommand command, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _bookingService.CreateAsync(userId, command, ct);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<BookingDto>> Update(int id, [FromBody] UpdateBookingCommand command, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _bookingService.UpdateAsync(userId, id, command, ct);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, [FromBody] DeleteBookingCommand command, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _bookingService.DeleteAsync(userId, id, command.RowVersion, ct);

            return NoContent();
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<BookingDto>> Cancel([FromBody] CancelBookingCommand command, CancellationToken ct)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _bookingService.CancelAsync(userId, command, ct);
            return Ok(result);
        }
    }
}