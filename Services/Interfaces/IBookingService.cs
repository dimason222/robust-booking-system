using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Dto.Responses;
using RobustBookingSystem.Models;

namespace RobustBookingSystem.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateAsync(int userId, CreateBookingCommand command, CancellationToken ct = default);
        Task<BookingDto> CancelAsync(int userId, CancelBookingCommand command, CancellationToken ct = default);
        Task<List<BookingDto>> GetMyBookingsAsync(int userId, CancellationToken ct = default);
        Task<List<BookingDto>> GetAllAsync(CancellationToken ct = default);
        Task<BookingDto> GetByIdAsync(int id, CancellationToken ct = default);
        Task<BookingDto> UpdateAsync(int userId, int bookingId, UpdateBookingCommand command, CancellationToken ct = default);
        Task DeleteAsync(int userId, int bookingId, byte[] rowVersion, CancellationToken ct = default);

    }
}
