namespace RobustBookingSystem.Dto.Commands
{
    public class UpdateBookingCommand
    {
        public int ResourceId { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
