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
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IBookingRepository bookingRepository,
            IResourceRepository resourceRepository,
            ILogger<BookingService> logger)
        {
            _bookingRepository = bookingRepository;
            _resourceRepository = resourceRepository;
            _logger = logger;
        }

        public async Task<List<BookingDto>> GetAllAsync(CancellationToken ct = default)
        {
            _logger.LogInformation("Request to get all bookings");

            var bookings = await _bookingRepository.GetAllAsync(ct);

            _logger.LogInformation("Retrieved {Count} bookings", bookings.Count);

            return bookings.Select(Map).ToList();
        }

        public async Task<BookingDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            _logger.LogInformation("Request to get booking {BookingId}", id);

            var booking = await _bookingRepository.GetByIdAsync(id, ct);

            if (booking is null)
            {
                _logger.LogWarning("Booking {BookingId} was not found", id);
                throw new NotFoundException("Бронювання не знайдено.");
            }

            return Map(booking);
        }

        public async Task<BookingDto> CreateAsync(int userId, CreateBookingCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "User {UserId} attempts to create booking for resource {ResourceId} from {StartAtUtc} to {EndAtUtc}",
                userId, command.ResourceId, command.StartAtUtc, command.EndAtUtc);

            if (command.StartAtUtc >= command.EndAtUtc)
            {
                _logger.LogWarning(
                    "Invalid booking interval from {StartAtUtc} to {EndAtUtc} for user {UserId}",
                    command.StartAtUtc, command.EndAtUtc, userId);

                throw new ValidationException("Час початку бронювання повинен бути меншим за час завершення.");
            }

            var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, ct);

            if (resource is null)
            {
                _logger.LogWarning(
                    "User {UserId} tried to create booking for non-existing resource {ResourceId}",
                    userId, command.ResourceId);

                throw new NotFoundException("Ресурс не знайдено.");
            }

            var hasConflict = await _bookingRepository.HasConflictAsync(
                command.ResourceId,
                command.StartAtUtc,
                command.EndAtUtc,
                ct);

            if (hasConflict)
            {
                _logger.LogWarning(
                    "Booking conflict detected for resource {ResourceId} from {StartAtUtc} to {EndAtUtc} by user {UserId}",
                    command.ResourceId, command.StartAtUtc, command.EndAtUtc, userId);

                throw new BookingConflictException("Обраний ресурс вже заброньований на вказаний часовий інтервал.");
            }

            var booking = new Booking
            {
                UserId = userId,
                ResourceId = command.ResourceId,
                StartAtUtc = command.StartAtUtc,
                EndAtUtc = command.EndAtUtc,
                Status = BookingStatus.Active
            };

            try
            {
                await _bookingRepository.AddAsync(booking, ct);

                var saved = await _bookingRepository.GetByIdAsync(booking.Id, ct)
                    ?? throw new NotFoundException("Не вдалося завантажити створене бронювання.");

                _logger.LogInformation(
                    "Booking {BookingId} created successfully by user {UserId} for resource {ResourceId}",
                    saved.Id, userId, command.ResourceId);

                return Map(saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while creating booking for user {UserId} and resource {ResourceId}",
                    userId, command.ResourceId);

                throw;
            }
        }

        public async Task<BookingDto> CancelAsync(int userId, CancelBookingCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "User {UserId} attempts to cancel booking {BookingId}",
                userId, command.BookingId);

            var booking = await _bookingRepository.GetByIdAsync(command.BookingId, ct);

            if (booking is null)
            {
                _logger.LogWarning(
                    "User {UserId} tried to cancel non-existing booking {BookingId}",
                    userId, command.BookingId);

                throw new NotFoundException("Бронювання не знайдено.");
            }

            if (booking.UserId != userId)
            {
                _logger.LogWarning(
                    "User {UserId} tried to cancel чужое booking {BookingId} owned by user {OwnerUserId}",
                    userId, command.BookingId, booking.UserId);

                throw new ForbiddenException("Користувач не може скасувати чуже бронювання.");
            }

            if (booking.Status != BookingStatus.Active)
            {
                _logger.LogWarning(
                    "User {UserId} tried to cancel booking {BookingId} with invalid status {Status}",
                    userId, command.BookingId, booking.Status);

                throw new ValidationException("Скасувати можна лише активне бронювання.");
            }

            booking.Status = BookingStatus.Cancelled;
            booking.RowVersion = command.RowVersion;

            try
            {
                await _bookingRepository.UpdateAsync(booking, ct);

                _logger.LogInformation(
                    "Booking {BookingId} successfully cancelled by user {UserId}",
                    booking.Id, userId);

                return Map(booking);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict while cancelling booking {BookingId} by user {UserId}",
                    command.BookingId, userId);

                throw new BookingConflictException("Бронювання було змінено іншим користувачем.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while cancelling booking {BookingId} by user {UserId}",
                    command.BookingId, userId);

                throw;
            }
        }
        public async Task<BookingDto> UpdateAsync(int userId, int bookingId, UpdateBookingCommand command, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "User {UserId} attempts to update booking {BookingId}",
                userId, bookingId);

            if (command.StartAtUtc >= command.EndAtUtc)
                throw new ValidationException("Час початку бронювання повинен бути меншим за час завершення.");

            var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);

            if (booking is null)
                throw new NotFoundException("Бронювання не знайдено.");

            if (booking.UserId != userId)
                throw new ForbiddenException("Користувач не може змінювати чуже бронювання.");

            if (booking.Status != BookingStatus.Active)
                throw new ValidationException("Змінювати можна лише активне бронювання.");

            var resource = await _resourceRepository.GetByIdAsync(command.ResourceId, ct);

            if (resource is null)
                throw new NotFoundException("Ресурс не знайдено.");

            var hasConflict = await _bookingRepository.HasConflictAsync(
                command.ResourceId,
                command.StartAtUtc,
                command.EndAtUtc,
                bookingId,
                ct);

            if (hasConflict)
                throw new BookingConflictException("Обраний ресурс вже заброньований на вказаний часовий інтервал.");

            booking.ResourceId = command.ResourceId;
            booking.StartAtUtc = command.StartAtUtc;
            booking.EndAtUtc = command.EndAtUtc;
            booking.RowVersion = command.RowVersion;

            try
            {
                await _bookingRepository.UpdateAsync(booking, ct);

                _logger.LogInformation(
                    "Booking {BookingId} updated successfully by user {UserId}",
                    bookingId, userId);

                var updated = await _bookingRepository.GetByIdAsync(bookingId, ct)
                    ?? throw new NotFoundException("Не вдалося завантажити оновлене бронювання.");

                return Map(updated);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict while updating booking {BookingId} by user {UserId}",
                    bookingId, userId);

                throw new BookingConflictException("Бронювання було змінено іншим користувачем.");
            }
        }

        public async Task DeleteAsync(int userId, int bookingId, byte[] rowVersion, CancellationToken ct = default)
        {
            _logger.LogInformation(
                "User {UserId} attempts to delete booking {BookingId}",
                userId, bookingId);

            var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);

            if (booking is null)
                throw new NotFoundException("Бронювання не знайдено.");

            if (booking.UserId != userId)
                throw new ForbiddenException("Користувач не може видалити чуже бронювання.");

            if (booking.Status != BookingStatus.Active)
                throw new ValidationException("Видалити можна лише активне бронювання.");

            booking.Status = BookingStatus.Cancelled;
            booking.RowVersion = rowVersion;

            try
            {
                await _bookingRepository.UpdateAsync(booking, ct);

                _logger.LogInformation(
                    "Booking {BookingId} was cancelled by DELETE request from user {UserId}",
                    bookingId, userId);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Concurrency conflict while deleting booking {BookingId} by user {UserId}",
                    bookingId, userId);

                throw new BookingConflictException("Бронювання було змінено іншим користувачем.");
            }
        }

        public async Task<List<BookingDto>> GetMyBookingsAsync(int userId, CancellationToken ct = default)
        {
            _logger.LogInformation("User {UserId} requests own bookings", userId);

            var bookings = await _bookingRepository.GetByUserAsync(userId, ct);

            _logger.LogInformation(
                "Returned {Count} bookings for user {UserId}",
                bookings.Count, userId);

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