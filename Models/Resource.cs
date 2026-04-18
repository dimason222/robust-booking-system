namespace RobustBookingSystem.Models
{
    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Capacity { get; set; }

        public List<Booking> Bookings { get; set; } = new();
    }
}
