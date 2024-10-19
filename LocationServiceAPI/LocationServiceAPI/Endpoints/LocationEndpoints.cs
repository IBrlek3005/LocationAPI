using LocationServiceAPI.Data;
using LocationServiceAPI.DTOs;
using LocationServiceAPI.Interfaces;
using LocationServiceAPI.Validations;
using Microsoft.AspNetCore.Mvc;

namespace LocationServiceAPI.Endpoints
{
    public static class LocationEndpoints
    {
        public static void MapLocationEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/locations", async ([FromBody] LocationRequestDTO request, ILocationService locationService) =>
            {
                var validator = new LocationRequestValidator();
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(error => error.ErrorMessage).ToList();
                    Results.BadRequest(errors);
                }
                var locations = await locationService.GetLocationsAsync(request);

                if (locations == null)
                {
                    return Results.NotFound(new { Message = "Nije pronađena lokacija." });
                }

                return Results.Ok(locations);
            }).RequireAuthorization("ApiKeyPolicy");

            app.MapGet("/api/getLocations", async (string? type, string? locationName, ILocationService locationService) =>
            {
                var searchDTO = new LocationSearchDTO
                {
                    Type = type,
                    LocationName = locationName
                };

                var locations = await locationService.GetLocationsFromDatabaseAsync(searchDTO);

                if (!locations.Any())
                {
                    return Results.NotFound(new { Message = "Nema pronađenih lokacija." });
                }

                return Results.Ok(locations);
            })
            .WithName("GetLocations")
            .Produces<List<LocationTypeDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("ApiKeyPolicy");

            app.MapPost("/api/SetFavouriteLocation", async ([FromBody] FavouriteLocationRequestDTO request, ILocationService locationService) =>
            {
                if (request == null || string.IsNullOrEmpty(request.LocationName))
                {
                    Results.NotFound("Nije predano ime lokacije.");
                }

                await locationService.SetFavouriteLocation(request);

                return Results.Ok();
            })
            .WithName("SetFavouriteLocation")
            .Produces<List<LocationTypeDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("ApiKeyPolicy");

            app.MapGet("/api/GetFavouriteLocations", async (ILocationService locationService) =>
            {
                var favouriteLocations = await locationService.GetFavouriteLocation();

                if (favouriteLocations == null || !favouriteLocations.Any())
                {
                    return Results.NotFound(new { Message = "Nema omiljenih lokacija za prijavljenog korisnika." });
                }

                return Results.Ok(favouriteLocations);
            })
            .WithName("GetFavouriteLocations")
            .Produces<List<LocationTypeDTO>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("ApiKeyPolicy");

        }
    }
}
