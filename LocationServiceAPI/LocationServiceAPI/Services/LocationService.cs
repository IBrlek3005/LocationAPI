using LocationServiceAPI.Data;
using LocationServiceAPI.DTOs;
using LocationServiceAPI.Hubs;
using LocationServiceAPI.Interfaces;
using LocationServiceAPI.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;

namespace LocationServiceAPI.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly IHubContext<SearchHub> _hubContext;
        private readonly IHttpContextAccessor _contextAccessor;
        public LocationService(HttpClient httpClient, IConfiguration configuration, AppDbContext context, IHubContext<SearchHub> hubContext, IHttpContextAccessor contextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _hubContext = hubContext;
            _contextAccessor = contextAccessor;
        }

        public async Task<LocationResponseDTO> GetLocationsAsync(LocationRequestDTO request)
        {
            await _semaphore.WaitAsync();
            try
            {
                var apiKey = _configuration.GetValue<string>("GoogleMaps:ApiKey");
                var response = await _httpClient.GetAsync($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={request.Latitude},{request.Longitude}&radius={request.Radius}&key={apiKey}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Došlo je do greške prilikom dohvata lokacije.");
                }

                var content = await response.Content.ReadAsStringAsync();
                var locationResponse = JsonConvert.DeserializeObject<LocationResponseDTO>(content);

                if (locationResponse == null || !locationResponse.results.Any())
                {
                    throw new InvalidOperationException("Nije dohvaćena niti jedna lokacija.");
                }

                await SaveLocationsToDatabase(locationResponse);
                return locationResponse;
            }
            finally
            {
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", "Nova pretraga izvršena za: " + request.Latitude + ", " + request.Longitude);
                _semaphore.Release();
            }
        }

        public async Task<List<LocationTypeDTO>> GetLocationsFromDatabaseAsync(LocationSearchDTO request)
        {
            await _semaphore.WaitAsync();
            try
            {
                var query = _context.Location
                    .Include(l => l.LocationTypes)
                    .ThenInclude(lt => lt.Type)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.LocationName))
                {
                    query = query.Where(l => l.Name == request.LocationName);
                }

                if (!string.IsNullOrEmpty(request.Type))
                {
                    query = query.Where(l => l.LocationTypes.Any(lt => lt.Type.Name == request.Type));
                }

                var locations = await query.Select(l => new LocationTypeDTO
                {
                    Name = l.Name,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Types = l.LocationTypes.Select(lt => lt.Type.Name).ToList()
                })
                .ToListAsync();

                return locations;
            }
           finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetFavouriteLocation(FavouriteLocationRequestDTO request)
        {
            await _semaphore.WaitAsync();
            try
            {
                var username = _contextAccessor.HttpContext.User.Identity.Name;
                var favouriteLocations = await _context.FavouriteLocations
                    .Include(fl => fl.User)
                    .Include(fl => fl.Location)
                    .FirstOrDefaultAsync(x => x.User.Username == username && x.Location.Name == request.LocationName);

                if (favouriteLocations != null) 
                {
                    throw new Exception("Lokacija već postoji u omiljenima.");
                }

                var newFavouriteLocation = new FavouriteLocation
                {
                    User = await _context.User.FirstOrDefaultAsync(u => u.Username == username),
                    Location = await _context.Location.FirstOrDefaultAsync(l => l.Name == request.LocationName)
                };

                if (newFavouriteLocation.User == null || newFavouriteLocation.Location == null)
                {
                    throw new Exception("Korisnik ili lokacija nisu pronađeni.");
                }

                _context.FavouriteLocations.Add(newFavouriteLocation);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<List<LocationTypeDTO>> GetFavouriteLocation()
        {
            await _semaphore.WaitAsync();
            try
            {
                var username = _contextAccessor.HttpContext.User.Identity.Name;
                var favouriteLocations = await _context.FavouriteLocations
                    .Include(fl => fl.User)
                    .Include(fl => fl.Location)
                    .Where(x => x.User.Username == username)
                    .Select(x => new LocationTypeDTO
                    {
                        Id = x.Location.Id,
                        Name = x.Location.Name,
                        Latitude = x.Location.Latitude,
                        Longitude = x.Location.Longitude,
                        Types = x.Location.LocationTypes.Select(t => t.Type.Name).ToList()
                    })
                    .ToListAsync();

                return favouriteLocations;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SaveLocationsToDatabase(LocationResponseDTO response)
        {
            foreach (var result in response.results)
            {
                var location = await GetOrCreateLocation(result);

                if (result.types != null)
                {
                    await SaveLocationTypes(location.Id, result.types);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task<Location> GetOrCreateLocation(ResultDTO result)
        {
            var existingLocation = await _context.Location.Where(l => l.Name == result.name
                    && l.Latitude == result.geometry.location.lat
                    && l.Longitude == result.geometry.location.lng)
                .FirstOrDefaultAsync();

            if (existingLocation != null)
            {
                return existingLocation;
            }

            var newLocation = new Location
            {
                Name = result.name,
                Latitude = result.geometry.location.lat,
                Longitude = result.geometry.location.lng
            };

            _context.Location.Add(newLocation);
            await _context.SaveChangesAsync();
            return newLocation;
        }

        private async Task<Models.Type> GetOrCreateType(string typeName)
        {
            var type = await _context.Type.Where(t => t.Name == typeName)
                .FirstOrDefaultAsync();

            if (type == null)
            {
                var newType = new Models.Type
                {
                    Id = Guid.NewGuid(),
                    Name = typeName,
                };
                _context.Type.Add(newType);
                await _context.SaveChangesAsync();
                return newType;
            }

            return type;
        }

        private async Task SaveLocationTypes(Guid locationId, List<string> types)
        {
            foreach (var typeName in types)
            {
                var type = await GetOrCreateType(typeName);

                var existingLocationType = await _context.LocationTypes.Where(lt => lt.LocationId == locationId && lt.TypeId == type.Id)
                    .FirstOrDefaultAsync();

                if (existingLocationType == null)
                {
                    var locationType = new LocationType
                    {
                        LocationId = locationId,
                        TypeId = type.Id
                    };

                    _context.LocationTypes.Add(locationType);
                }
            }
        }
    }
}
