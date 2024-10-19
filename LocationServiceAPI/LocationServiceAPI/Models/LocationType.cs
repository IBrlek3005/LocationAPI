using System.ComponentModel.DataAnnotations;

namespace LocationServiceAPI.Models
{
    public class LocationType
    {
        [Key]
        public Guid LocationId { get; set; }
        public Location? Location { get; set; }

        [Key]
        public Guid TypeId { get; set; }
        public Type? Type { get; set; }
    }
}
