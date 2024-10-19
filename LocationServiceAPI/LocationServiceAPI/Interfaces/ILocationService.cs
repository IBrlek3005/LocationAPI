using LocationServiceAPI.DTOs;

namespace LocationServiceAPI.Interfaces
{
    public interface ILocationService
    {
        public Task<LocationResponseDTO> GetLocationsAsync(LocationRequestDTO request);
        public Task<List<LocationTypeDTO>> GetLocationsFromDatabaseAsync(LocationSearchDTO request);
        public Task SetFavouriteLocation(FavouriteLocationRequestDTO request);
        public Task<List<LocationTypeDTO>> GetFavouriteLocation();
    }
}
