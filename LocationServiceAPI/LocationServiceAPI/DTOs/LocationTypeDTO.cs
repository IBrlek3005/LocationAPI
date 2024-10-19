namespace LocationServiceAPI.DTOs
{
    public class LocationTypeDTO
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<string>? Types { get; set; }
    }
}
