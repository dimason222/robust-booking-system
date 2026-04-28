using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Authorization;
using RobustBookingSystem.Data;
using RobustBookingSystem.Dto.Auth;
using RobustBookingSystem.Models;
using RobustBookingSystem.Services.Interfaces;

namespace RobustBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        {
            var result = await _authService.LoginAsync(dto, ct);

            if (result is null)
                return Unauthorized(new { message = "Невірний email або пароль." });

            return Ok(result);
        }
    }
}