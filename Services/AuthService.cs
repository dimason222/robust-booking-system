using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RobustBookingSystem.Authorization;
using RobustBookingSystem.Data;
using RobustBookingSystem.Dto.Auth;
using RobustBookingSystem.Models;
using RobustBookingSystem.Services.Interfaces;

namespace RobustBookingSystem.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            JwtTokenGenerator jwtTokenGenerator,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthService> logger)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct)
        {
            _logger.LogInformation("Login attempt for email {Email}", dto.Email);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == dto.Email, ct);

                if (user is null)
                {
                    _logger.LogWarning("Login failed: user with email {Email} not found", dto.Email);
                    return null;
                }

                var verificationResult = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password);

                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning(
                        "Login failed: invalid password for user {UserId} ({Email})",
                        user.Id, user.Email);

                    return null;
                }

                var token = _jwtTokenGenerator.GenerateToken(user);

                _logger.LogInformation(
                    "User {UserId} ({Email}) successfully logged in",
                    user.Id, user.Email);

                return new AuthResponseDto
                {
                    Token = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error during login for email {Email}",
                    dto.Email);

                throw;
            }
        }
    }
}