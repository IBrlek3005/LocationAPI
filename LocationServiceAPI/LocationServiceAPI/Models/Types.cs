using System.ComponentModel.DataAnnotations;

namespace LocationServiceAPI.Models
{
    public class Type
    {
        [Key] public Guid Id { get; set; }
        public string? Name { get; set; }
        public ICollection<LocationType>? LocationTypes { get; set; }
    }
}
