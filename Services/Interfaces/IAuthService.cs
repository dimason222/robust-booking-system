using RobustBookingSystem.Dto.Auth;

namespace RobustBookingSystem.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct);
    }
}
