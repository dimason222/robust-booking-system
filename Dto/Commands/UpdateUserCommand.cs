using RobustBookingSystem.Models;

namespace RobustBookingSystem.Dto.Commands
{
    public class UpdateUserCommand
    {
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public string? Password { get; set; }
    }
}
