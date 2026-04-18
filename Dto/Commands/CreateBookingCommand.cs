namespace RobustBookingSystem.Dto.Commands
{
    public class CreateBookingCommand
    {
        public int UserId { get; set; } // remove later
        public int ResourceId { get; set; }
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
    }
}
