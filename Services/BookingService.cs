using Microsoft.EntityFrameworkCore;
using RobustBookingSystem.Dto.Commands;
using RobustBookingSystem.Dto.Responses;
using RobustBookingSystem.Exceptions;
using RobustBookingSystem.Models;
using RobustBookingSystem.Repositories.Interfaces;
using RobustBookingSystem.Services.Interfaces;

namespace RobustBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IResourceRepository _resourceRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IResourceRepository resourceRepository)
        {
            _bookingRepository = bookingRepository;
            _resourceRepository = resourceRepository;
        }

        public async Task<List<BookingDto>> GetAllAsync(CancellationToken ct = default)
        {
            var bookings = await _bookingRepository.GetAllAsync(ct);
            return bookings.Select(Map).ToList();
        }

        public async Task<BookingDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(id, ct);

            if (booking is null)
                throw new NotFoundException("Бронювання не знайдено.");

            return Map(booking);
        }

        public async Task<BookingDto> CreateAsync(int userId, CreateBookingCommand command, CancellationToken ct = default)
        {
            if (command.StartAtUtc >= command.EndAtUtc)
                throw new ValidationException("Час початку бронювання повинен бути меншим за час завершення.");

            var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, ct);
            if (resource is null)
                throw new NotFoundException("Ресурс не знайдено.");

            var hasConflict = await _bookingRepository.HasConflictAsync(
                command.ResourceId,
                command.StartAtUtc,
                command.EndAtUtc,
                ct);

            if (hasConflict)
                throw new BookingConflictException("Обраний ресурс вже заброньований на вказаний часовий інтервал.");

            var booking = new Booking
            {
                UserId = userId,
                ResourceId = command.ResourceId,
                StartAtUtc = command.StartAtUtc,
                EndAtUtc = command.EndAtUtc,
                Status = BookingStatus.Active
            };

            await _bookingRepository.AddAsync(booking, ct);

            var saved = await _bookingRepository.GetByIdAsync(booking.Id, ct)
                ?? throw new NotFoundException("Не вдалося завантажити створене бронювання.");

            return Map(saved);
        }

        public async Task<BookingDto> CancelAsync(int userId, CancelBookingCommand command, CancellationToken ct = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, ct);

            if (booking is null)
                throw new NotFoundException("Бронювання не знайдено.");

            if (booking.UserId != userId)
                throw new ForbiddenException("Користувач не може скасувати чуже бронювання.");

            if (booking.Status != BookingStatus.Active)
                throw new ValidationException("Скасувати можна лише активне бронювання.");

            booking.Status = BookingStatus.Cancelled;
            booking.RowVersion = command.RowVersion;

            try
            {
                await _bookingRepository.UpdateAsync(booking, ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new BookingConflictException("Бронювання було змінено іншим користувачем.");
            }

            return Map(booking);
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync(int userId, CancellationToken ct = default)
        {
            var bookings = await _bookingRepository.GetByUserAsync(userId, ct);
            return bookings.Select(Map).ToList();
        }

        private static BookingDto Map(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                ResourceId = booking.ResourceId,
                ResourceName = booking.Resource?.Name ?? string.Empty,
                UserId = booking.UserId,
                UserName = booking.User?.FullName ?? string.Empty,
                StartAtUtc = booking.StartAtUtc,
                EndAtUtc = booking.EndAtUtc,
                Status = booking.Status.ToString(),
                RowVersion = booking.RowVersion
            };
        }
    }
}