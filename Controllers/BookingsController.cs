using Microsoft.AspNetCore.Mvc;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Dto.Responses;
using RobustBookingSystem.Services.Interfaces;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BookingDto>>> GetAll(CancellationToken ct)
        {
            var result = await _bookingService.GetAllAsync(ct);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BookingDto>> GetById(int id, CancellationToken ct)
        {
            var result = await _bookingService.GetByIdAsync(id, ct);
            return Ok(result);
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<List<BookingDto>>> GetByUser(int userId, CancellationToken ct)
        {
            var result = await _bookingService.GetMyBookingsAsync(userId, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingCommand command, CancellationToken ct)
        {
            var result = await _bookingService.CreateAsync(command.UserId, command, ct);
            return Ok(result);
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<BookingDto>> Cancel([FromBody] CancelBookingCommand command, CancellationToken ct)
        {
            // временно без JWT можно просто использовать userId из тела команды,
            // но если у тебя его там нет, пока поставь тестовый вариант:
            var userId = 1;

            var result = await _bookingService.CancelAsync(userId, command, ct);
            return Ok(result);
        }
    }
}