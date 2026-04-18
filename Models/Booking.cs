using System.ComponentModel.DataAnnotations;

namespace RobustBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ResourceId { get; set; }
        public Resource Resource { get; set; } = null!;

        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Active;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
