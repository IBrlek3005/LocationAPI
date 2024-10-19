using LocationServiceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LocationServiceAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Location> Location { get; set; }
        public DbSet<Models.Type> Type { get; set; }
        public DbSet<LocationType> LocationTypes { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<FavouriteLocation> FavouriteLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LocationType>()
                .HasKey(lt => new { lt.LocationId, lt.TypeId });

            modelBuilder.Entity<LocationType>()
                .HasOne(lt => lt.Location)
                .WithMany(l => l.LocationTypes)
                .HasForeignKey(lt => lt.LocationId);

            modelBuilder.Entity<LocationType>()
                .HasOne(lt => lt.Type)
                .WithMany(t => t.LocationTypes)
                .HasForeignKey(lt => lt.TypeId);
        }
    }
}
