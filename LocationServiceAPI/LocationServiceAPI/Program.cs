using LocationServiceAPI.Data;
using LocationServiceAPI.Endpoints;
using LocationServiceAPI.Hubs;
using LocationServiceAPI.Interfaces;
using LocationServiceAPI.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>
        policy.RequireAuthenticatedUser());
});


AddLocationServices(builder.Services);

var app = builder.Build();


app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<LocationServiceAPI.Middlewares.AuthenticationMiddleware>();
app.MapHub<SearchHub>("/searchHub");
app.UseHttpsRedirection();
app.MapLocationEndpoints();
app.MapAuthorizationEndpoints();

app.Run();

void AddLocationServices(IServiceCollection services)
{
    services.AddHttpClient();
    services.AddHttpContextAccessor();
    services.AddScoped<ILocationService, LocationService>();
}
