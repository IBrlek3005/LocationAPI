namespace LocationServiceAPI.Models
{
    public class FavouriteLocation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid LocationId { get; set; }

        public User? User { get; set; }
        public Location? Location { get; set; }
    }
}
