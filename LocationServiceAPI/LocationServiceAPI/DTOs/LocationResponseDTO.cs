namespace LocationServiceAPI.DTOs
{
    public class LocationResponseDTO
    {
        public List<ResultDTO>? results { get; set; }
        public string? status { get; set; }
    }

    public class ResultDTO
    {
        public string? name { get; set; }
        public GeometryDTO? geometry { get; set; }
        public List<string>? types { get; set; }
    }

    public class GeometryDTO
    {
        public LocationDTO? location { get; set; }
    }

    public class LocationDTO
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
}
