using RobustBookingSystem.Dto.Auth;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Services.Users
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync(CancellationToken ct);
        Task<User?> GetByIdAsync(int id, CancellationToken ct);
        Task<(bool Success, string? Error, User? User)> CreateAsync(RegisterRequestDto dto, CancellationToken ct);
        Task<(bool Success, string? Error, User? User)> UpdateAsync(int id, RegisterRequestDto dto, CancellationToken ct);
        Task DeleteAsync(int id, CancellationToken ct);
    }
}