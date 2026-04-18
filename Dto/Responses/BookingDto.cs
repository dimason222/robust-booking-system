namespace RobustBookingSystem.Dto.Responses
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public string ResourceName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime StartAtUtc { get; set; }
        public DateTime EndAtUtc { get; set; }
        public string Status { get; set; } = string.Empty;
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
