using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Data;
using RobustBookingSystem.Dto.Auth;
using RobustBookingSystem.Exceptions;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Services.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<UserService> _logger;

        public UserService(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher,
            ILogger<UserService> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken ct)
        {
            _logger.LogInformation("Request to get all users");

            var users = await _context.Users.ToListAsync(ct);

            _logger.LogInformation("Retrieved {Count} users", users.Count);

            return users;
        }

        public async Task<User?> GetByIdAsync(int id, CancellationToken ct)
        {
            _logger.LogInformation("Request to get user {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, ct);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found", id);
                return null;
            }

            return user;
        }

        public async Task<(bool Success, string? Error, User? User)> CreateAsync(RegisterRequestDto dto, CancellationToken ct)
        {
            _logger.LogInformation(
                "Attempt to create user with email {Email} and role {Role}",
                dto.Email, dto.Role);

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == dto.Email, ct);

            if (existingUser is not null)
            {
                _logger.LogWarning(
                    "User creation failed: email {Email} already exists",
                    dto.Email);

                return (false, "Користувач з таким email вже існує.", null);
            }

            var user = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = (UserRole)dto.Role
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "User {UserId} with email {Email} created successfully",
                    user.Id, user.Email);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating user with email {Email}",
                    dto.Email);

                throw;
            }
        }

        public async Task<(bool Success, string? Error, User? User)> UpdateAsync(int id, RegisterRequestDto dto, CancellationToken ct)
        {
            _logger.LogInformation(
                "Attempt to update user {UserId}",
                id);

            var user = await _context.Users.FindAsync(new object[] { id }, ct);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found for update", id);
                return (false, "Користувача не знайдено.", null);
            }

            // Проверка email (если меняется)
            if (user.Email != dto.Email)
            {
                var emailExists = await _context.Users
                    .AnyAsync(x => x.Email == dto.Email && x.Id != id, ct);

                if (emailExists)
                {
                    _logger.LogWarning(
                        "User update failed: email {Email} already exists",
                        dto.Email);

                    return (false, "Користувач з таким email вже існує.", null);
                }
            }

            user.Email = dto.Email;
            user.FullName = dto.FullName;
            user.Role = (UserRole)dto.Role;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);
            }

            try
            {
                await _context.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "User {UserId} updated successfully",
                    user.Id);

                return (true, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while updating user {UserId}",
                    id);

                throw;
            }
        }

        public async Task DeleteAsync(int id, CancellationToken ct)
        {
            _logger.LogInformation("Attempt to delete user {UserId}", id);

            var user = await _context.Users.FindAsync(new object[] { id }, ct);

            if (user is null)
            {
                _logger.LogWarning("User {UserId} not found for deletion", id);
                throw new NotFoundException("Користувача не знайдено.");
            }

            var hasBookings = await _context.Bookings
                .AnyAsync(b => b.UserId == id, ct);

            if (hasBookings)
            {
                _logger.LogWarning(
                    "User {UserId} cannot be deleted because user has related bookings",
                    id);

                throw new ConflictException(
                    "Користувача не можна видалити, оскільки він має пов'язані бронювання.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("User {UserId} deleted successfully", id);
        }
    }
}