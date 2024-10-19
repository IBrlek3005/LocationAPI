using LocationServiceAPI.Data;
using LocationServiceAPI.DTOs;
using LocationServiceAPI.Helpers;
using LocationServiceAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocationServiceAPI.Endpoints
{
    public static class AuthorizationEndpoint
    {
        public static void MapAuthorizationEndpoints(this WebApplication app)
        {
            app.MapPost("/api/register", async ([FromBody] RegisterRequestDTO request, AppDbContext context) =>
            {
                if (await context.User.AnyAsync(u => u.Username == request.Username))
                {
                    return Results.BadRequest(new { Message = "Korisničko ime je već zauzeto." });
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = request.Username,
                    PasswordHash = PasswordHelper.HashPassword(request.Password),
                    ApiKey = Guid.NewGuid().ToString()
                };

                context.User.Add(user);
                await context.SaveChangesAsync();

                return Results.Created($"/api/users/{user.Id}", user);
            }).AllowAnonymous();
        }
    }
}
