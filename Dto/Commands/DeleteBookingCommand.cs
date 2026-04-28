namespace RobustBookingSystem.Dto.Commands
{
    public class DeleteBookingCommand
    {
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
