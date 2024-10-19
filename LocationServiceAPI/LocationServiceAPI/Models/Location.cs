using System.ComponentModel.DataAnnotations;

namespace LocationServiceAPI.Models
{
    public class Location
    {
        [Key]
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ICollection<LocationType>? LocationTypes { get; set; }
    }
}
