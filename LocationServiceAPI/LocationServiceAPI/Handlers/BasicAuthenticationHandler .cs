using LocationServiceAPI.Data;
using LocationServiceAPI.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly AppDbContext _context;
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        AppDbContext context) : base(options, logger, encoder, clock)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (authorizationHeader.Count == 0 ||
            !authorizationHeader.ToString().StartsWith("Basic "))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authorizationHeader.ToString().Substring("Basic ".Length).Trim();
        var credentialBytes = Convert.FromBase64String(token);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
        var username = credentials[0];
        var password = credentials[1];

        if (IsValidUser(username, password))
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Invalid authentication credentials");
    }

    private bool IsValidUser(string username, string password)
    {
        var user = _context.User.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return false;
        }

        return PasswordHelper.VerifyPassword(password, user.PasswordHash);
    }
}
