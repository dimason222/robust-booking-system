namespace RobustBookingSystem.Dto.Commands
{
    public class CancelBookingCommand
    {
        public int BookingId { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
