using LocationServiceAPI.Data;
using LocationServiceAPI.Helpers;
using System.Net;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace LocationServiceAPI.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();

            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                // Preskoči autentifikaciju za anonimne rute
                await _next(context);
                return;
            }

            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Ne autoriziran.");
                return;
            }

            var header = context.Request.Headers["Authorization"];
            var encodedCreds = header.ToString().Substring(6);
            var creds = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCreds));

            string[] uidpwd = creds.Split(':');
            var uid = uidpwd[0];
            var pwd = uidpwd[1];

            var user = await context.RequestServices.GetRequiredService<AppDbContext>()
                    .User.FirstOrDefaultAsync(u => u.Username == uid);

            if (user == null || !PasswordHelper.VerifyPassword(pwd, user.PasswordHash))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync("Netočno korisničko ime ili lozinka.");
                return;
            }

            await _next(context);
        }
    }
}
